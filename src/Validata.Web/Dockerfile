FROM node:22-alpine AS build
WORKDIR /app

COPY src/Validata.Web/package.json src/Validata.Web/package-lock.json* ./
RUN npm ci

COPY src/Validata.Web/ .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist/validata-web/browser /usr/share/nginx/html
COPY src/Validata.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
