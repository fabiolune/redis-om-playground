FROM lipanski/docker-static-website:2.4.0 AS httpd

FROM node:23.9-alpine AS build

WORKDIR /app

COPY redis.om.playground.ui/package*.json ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci --cache /root/.npm

COPY redis.om.playground.ui/ ./

RUN npm run build_client

COPY --from=httpd /home/static ./

RUN mv httpd.conf dist/public/httpd.conf
RUN sed -i 's|/assets|\./assets|g' dist/public/index.html

FROM lipanski/docker-static-website:2.4.0 AS static
USER static
HEALTHCHECK NONE

WORKDIR /home/static

COPY --from=build /app/dist/public ./

EXPOSE 3000