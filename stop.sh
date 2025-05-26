#!/bin/bash
# This script stops a kind cluster and removes the associated registry container.
# It assumes that the kind cluster and registry were created with the names 'test-kind' and 'kind-registry' respectively.
# Usage: ./stop.sh

CLUSTER_NAME=test-kind
kind delete cluster --name ${CLUSTER_NAME}

reg_name='kind-registry'
docker stop ${reg_name}
docker rm ${reg_name}