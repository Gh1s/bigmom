#stage 1
FROM node:14-buster as node
COPY ["ClientApp/", "/app"]
WORKDIR /app
#RUN npm install --pure-lockfile
RUN npm install 
#RUN npm run build --prod
RUN npm run build
#stage 2
FROM nginx:1.19.5-alpine
COPY --from=node /app/dist/bigmom-app /usr/share/nginx/html
#EXPOSE 80
COPY nginx.conf /etc/nginx/conf.d/default.conf
