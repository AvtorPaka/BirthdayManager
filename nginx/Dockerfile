FROM nginx:alpine
EXPOSE 443

COPY ./ssl /etc/nginx/certs
COPY ngnix.conf /etc/nginx/nginx.conf

ENTRYPOINT ["nginx", "-g", "daemon off;"]