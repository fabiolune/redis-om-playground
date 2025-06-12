#!/bin/bash
# This script stops a kind cluster and removes the associated registry container.
# It assumes that the kind cluster and registry were created with the names 'test-kind' and 'kind-registry' respectively.
# The script then creates a redis cluster and delivers the redis-om-playground app and runs some integration tests
# Usage: ./run.sh  

skip_build=false
if [ "$1" == "--skip-build" ]; then
  skip_build=true
fi

CLUSTER_PORT=80
CLUSTER_NAME=test-kind

API_PREFIX=person-api
API_BASE_PATH="http://localhost:${CLUSTER_PORT}/${API_PREFIX}"

UI_BASE_PATH="http://localhost:${CLUSTER_PORT}"

BLUE='\033[1;34m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

# --------------------------------------
# Useful functions definition
# --------------------------------------

log() {
  echo -e "\n$(date +%T) - $@\n"
}

info() {
  log "${BLUE}[INFO]${NC} $@"
}

success() {
  log "${GREEN}[SUCCESS]${NC} $@"
}

error() {
  >&2 log "${RED}[ERROR]${NC} $@"
}

validate_variable() {
  var_name=$1
  var_value=$(echo ${!var_name})
  if [ -z "$var_value" ]; then
    error "missing required environment variable $var_name"
    exit 1
  fi
}

validate_value() {
  json=$1
  key=$2
  expected_value=$3
  result=$(docker run --rm -i imega/jq -e '.'$key' == "'$expected_value'"' <<< "$json")

  if [ ! "$result" = "true" ]; then
      echo "❌ Validation failed: '$key' is not set to '$expected_value'"
      exit 1
  fi
}

preload_images_and_install() {
  local helm_chart=$1
  local helm_values=$2
  local helm_name=$3

  << EOF $h template ${helm_chart} -f - | awk "/image:/" | sed 's/image://g' | sed 's/\s//g' | sed 's/"//g' | xargs
 -i sh -c "docker pull {} && kind load docker-image --name $KIND_NAME {}"
${helm_values}
EOF

  << EOF $h upgrade --install --wait --debug --timeout 600s ${helm_name} ${helm_chart} -f -
${helm_values}
EOF

}

check_command() {
  command -v "$1" &> /dev/null
  if [ $? -ne 0 ]; then
    error "Command '$1' not found. Please install it first."
    exit 1
  fi
}

validate_status_code() {
  local url=$1
  local expected_code=$2
  local retries=30
  for i in $(seq 1 $retries); do
    sleep 1
    status_code=$(curl -i -s -o /dev/null -w "%{http_code}" $url)
    if [[ $status_code = $expected_code ]]; then
      success "SThe http call to '$url' correctly returned status code $expected_code"
      break
    else
      if [[ "$retries" = "$i" ]]; then
    error "❌ The http call to '$url' returned status code ${status_code}, $expected_code was expected"
    exit 1
      fi
    fi
  done
}

create_and_check_user() {
  local first_name=$1
  local last_name=$2
  local statement=$3

  raw_person=$(curl -s ${API_BASE_PATH}/person \
    -H 'Content-Type: application/json' \
    -d "{
        \"firstName\": \"${first_name}\",
        \"lastName\": \"${last_name}\",
        \"personalStatement\": \"${statement}\"
    }")
  person_id=$(echo $raw_person | cut -d':' -f2 | cut -d'"' -f1)

  person=$(curl -s -H 'Content-Type: application/json' ${API_BASE_PATH}/person/${person_id})

  if ! echo "$person" | jq -e . >/dev/null 2>&1; then
    error "Invalid JSON response for person:"
    error "$person"
    exit 1
  fi
  
  validate_value "$person" "id" "$person_id"
  validate_value "$person" "firstName" "$first_name"
  validate_value "$person" "lastName" "$last_name"

  local raw_search_results=$(curl -s -H 'Content-Type: application/json' \
    "${API_BASE_PATH}/person/search?q=${first_name}")

  if ! echo "$raw_search_results" | jq -e . >/dev/null 2>&1; then
    error "Invalid JSON response for search:"
    echo "$raw_search_results"
    exit 1
  fi

  count=$(echo "$raw_search_results" | jq "[.data | .[] | select(.firstName == \"$first_name\") | select(.id == \"$person_id\") ] | length")

  if [ "$count" -lt 1 ]; then
    error "No user found with firstName = $first_name"
    exit 1
  fi

  echo $person_id
}

delete_user() {
  local person_id=$1
  response=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE ${API_BASE_PATH}/person/${person_id})

  if [ "$response" != "202" ]; then
    error "Failed to delete user with ID $person_id, status code: $response"
    exit 1
  fi

  success "User with ID $person_id successfully deleted"
}

# --------------------------------------
# Commands check
# --------------------------------------

check_command kind
check_command kubectl
check_command helm
check_command docker
check_command base64
check_command sed
check_command curl
check_command jq
check_command cut
check_command tr

# --------------------------------------
# Create local registry and cluster
# --------------------------------------

reg_name='kind-registry'
reg_port='5001'
if [ "$(docker inspect -f '{{.State.Running}}' "${reg_name}" 2>/dev/null || true)" != 'true' ]; then
  docker run \
    -d --restart=always -p "127.0.0.1:${reg_port}:5000" --network bridge --name "${reg_name}" \
    registry:2
fi

kind create cluster --wait 300s --name ${CLUSTER_NAME} --config=- <<EOF
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
containerdConfigPatches:
- |-
  [plugins."io.containerd.grpc.v1.cri".registry]
    config_path = "/etc/containerd/certs.d"
nodes:
- role: control-plane
  kubeadmConfigPatches:
  - |
    kind: InitConfiguration
    nodeRegistration:
      kubeletExtraArgs:
    node-labels: "ingress-ready=true"
  extraMounts:
  - hostPath: ${PWD}/data
    containerPath: /data
  extraPortMappings:
  - containerPort: 80
    hostPort: ${CLUSTER_PORT}
    protocol: TCP
EOF

REGISTRY_DIR="/etc/containerd/certs.d/localhost:${reg_port}"
for node in $(kind get nodes --name ${CLUSTER_NAME}); do
  docker exec "${node}" mkdir -p "${REGISTRY_DIR}"
  cat <<EOF | docker exec -i "${node}" cp /dev/stdin "${REGISTRY_DIR}/hosts.toml"
[host."http://${reg_name}:5000"]
EOF
done

if [ "$(docker inspect -f='{{json .NetworkSettings.Networks.kind}}' "${reg_name}")" = 'null' ]; then
  docker network connect "kind" "${reg_name}"
fi

cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: ConfigMap
metadata:
  name: local-registry-hosting
  namespace: kube-public
data:
  localRegistryHosting.v1: |
    host: "localhost:${reg_port}"
    help: "https://kind.sigs.k8s.io/docs/user/local-registry/"
EOF

# --------------------------------------
# build image
# --------------------------------------

fullSemVer=$(docker run --rm -it -v $(pwd):/repo gittools/gitversion /repo | sed "1s/.*/\{/" | jq -r '.FullSemVer')

if [ "$skip_build" = true ]; then
  info "Skipping build step as requested"
else
  info "Building images with fullSemVer: $fullSemVer"
  docker build . \
    -t redis-om-playground-api:${fullSemVer} \
    -t localhost:5001/redis-om-playground-api:${fullSemVer} \
    -t localhost:5001/redis-om-playground-api:latest -f Api.Dockerfile

  docker build . \
    -t redis-om-playground-ui:${fullSemVer} \
    -t localhost:5001/redis-om-playground-ui:${fullSemVer} \
    -t localhost:5001/redis-om-playground-ui:latest -f UI.Dockerfile

  docker push localhost:5001/redis-om-playground-api:${fullSemVer}
  docker push localhost:5001/redis-om-playground-ui:${fullSemVer}
  docker push localhost:5001/redis-om-playground-api:latest
  docker push localhost:5001/redis-om-playground-ui:latest
fi

# --------------------------------------
# Tests execution
# --------------------------------------

k="kubectl --context kind-$CLUSTER_NAME"
h="helm --kube-context kind-$CLUSTER_NAME"

info "Install nginx ingress controller"
$k \
  apply \
  -f https://kind.sigs.k8s.io/examples/ingress/deploy-ingress-nginx.yaml

$k wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=90s

info "Installing Redis Cluster chart"

chartValues=$(
  cat <<EOF
redis:
  defaultConfigOverride: |
    loadmodule /opt/bitnami/redis/lib/redis/modules/redisbloom.so
    loadmodule /opt/bitnami/redis/lib/redis/modules/redisearch.so
    loadmodule /opt/bitnami/redis/lib/redis/modules/rejson.so
    loadmodule /opt/bitnami/redis/lib/redis/modules/redistimeseries.so
    
    bind 127.0.0.1 -::1
    protected-mode yes
    port 6379
    tcp-backlog 511
    timeout 0
    tcp-keepalive 300
    daemonize no
    pidfile /opt/bitnami/redis/tmp/redis_6379.pid
    loglevel notice
    logfile ""
    databases 16
    always-show-logo yes
    set-proc-title yes
    proc-title-template "{title} {listen-addr} {server-mode}"
    save 900 1 300 10 60 10000
    stop-writes-on-bgsave-error yes
    rdbcompression yes
    rdbchecksum yes
    dbfilename dump.rdb
    rdb-del-sync-files no
    dir /bitnami/redis/data
    replica-serve-stale-data yes
    replica-read-only yes
    repl-diskless-sync no
    repl-diskless-sync-delay 5
    repl-diskless-sync-max-replicas 0
    repl-diskless-load disabled
    repl-disable-tcp-nodelay no
    replica-priority 100
    acllog-max-len 128
    lazyfree-lazy-eviction no
    lazyfree-lazy-expire no
    lazyfree-lazy-server-del no
    replica-lazy-flush no
    lazyfree-lazy-user-del no
    lazyfree-lazy-user-flush no
    oom-score-adj no
    oom-score-adj-values 0 200 800
    disable-thp yes
    appendonly no
    appendfilename "appendonly.aof"
    appenddirname "appendonlydir"
    appendfsync everysec
    no-appendfsync-on-rewrite no
    auto-aof-rewrite-percentage 100
    auto-aof-rewrite-min-size 64mb
    aof-load-truncated yes
    aof-use-rdb-preamble yes
    aof-timestamp-enabled no
    lua-time-limit 5000
    cluster-enabled yes
    cluster-config-file /bitnami/redis/data/nodes.conf
    slowlog-log-slower-than 10000
    slowlog-max-len 128
    latency-monitor-threshold 0
    notify-keyspace-events ""
    hash-max-listpack-entries 512
    hash-max-listpack-value 64
    list-max-listpack-size -2
    list-compress-depth 0
    set-max-intset-entries 512
    zset-max-listpack-entries 128
    zset-max-listpack-value 64
    hll-sparse-max-bytes 3000
    stream-node-max-bytes 4096
    stream-node-max-entries 100
    activerehashing yes
    client-output-buffer-limit normal 0 0 0
    client-output-buffer-limit replica 256mb 64mb 60
    client-output-buffer-limit pubsub 32mb 8mb 60
    hz 10
    dynamic-hz yes
    aof-rewrite-incremental-fsync yes
    rdb-save-incremental-fsync yes
    jemalloc-bg-thread yes
EOF
)

temp_values=$(mktemp)
echo "$chartValues" > $temp_values

$h upgrade -i --wait --timeout 600s redis-cluster oci://registry-1.docker.io/bitnamicharts/redis-cluster -f $temp_values 2> /dev/null

rm $temp_values

info "Create secret to store redis connection string"

$k delete secret redis-authentication > /dev/null 2>&1

redis_password=$($k get secret redis-cluster -o jsonpath="{.data.redis-password}" | base64 --decode)
connection_string="redis-cluster-headless:6379,password=${redis_password},abortConnect=false"

$k create secret generic redis-authentication \
    --from-literal connection-string="${connection_string}"

timestamp=$(date +%s%N | cut -b1-13)
info "Install API"

cat ./manifests/api-deployment.yaml | sed 's/<pathbase>/'${API_PREFIX}'/g' | sed 's/<timestamp>/'${timestamp}'/g' | $k apply -f -
$k wait \
  --for=condition=ready pod \
  --selector=app=redis-om-playground-api \
  --timeout=90s

$k apply -f ./manifests/api-service.yaml
cat ./manifests/api-ingress.yaml | sed 's/<pathbase>/'${API_PREFIX}'/g' | $k apply -f -

info "Test API is reachable"
validate_status_code "$API_BASE_PATH/internal/description" 200

info "Install UI"


cat ./manifests/ui-configmap.yaml | sed 's|<apibaseurl>|'${API_BASE_PATH}'|g' | $k apply -f -
cat ./manifests/ui-deployment.yaml | sed 's|<timestamp>|'${timestamp}'|g' | $k apply -f -
$k apply -f ./manifests/ui-service.yaml
$k apply -f ./manifests/ui-ingress.yaml

$k wait \
  --for=condition=ready pod \
  --selector=app=redis-om-playground-ui \
  --timeout=90s

info "Test UI is reachable"
validate_status_code "$UI_BASE_PATH/index.html" 200

echo -e "${YELLOW}------------------------
╔═╗╔═╗╦  ┌┬┐┌─┐┌─┐┌┬┐┌─┐
╠═╣╠═╝║   │ ├┤ └─┐ │ └─┐
╩ ╩╩  ╩   ┴ └─┘└─┘ ┴ └─┘
------------------------${NC}"

statement="Lorem ipsum dolor sit amet"

first_name_1="first_name_1"
last_name_1="last_name_1"
info "Creating user with first name: '$first_name_1', last name: '$last_name_1', personal statement: '$statement'"
id_1=$(create_and_check_user "$first_name_1" "$last_name_1" "$statement") || exit 1
success "User successfully created with ID '$id_1'"

first_name_2="first_name_2"
last_name_2="last_name_2"
info "Creating user with first name: '$first_name_2', last name: '$last_name_2', personal statement: '$statement'"
id_2=$(create_and_check_user "$first_name_2" "$last_name_2" "$statement") || exit 1
success "User successfully created with ID '$id_2'"

delete_user "$id_1" || exit 1
delete_user "$id_2" || exit 1

echo -e "${YELLOW}---------------------
╦ ╦╦  ╔╦╗┌─┐┌─┐┌┬┐┌─┐
║ ║║   ║ ├┤ └─┐ │ └─┐
╚═╝╩   ╩ └─┘└─┘ ┴ └─┘
---------------------${NC}"