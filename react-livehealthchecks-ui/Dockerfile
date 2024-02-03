FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
RUN apk add nodejs
RUN apk add npm

WORKDIR /app
COPY react-livehealthchecks-ui/package.json ./
COPY react-livehealthchecks-ui/. ./

RUN npm install

RUN npm run build

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=build-env /app/build .
COPY react-livehealthchecks-ui/nginx.conf /etc/nginx/nginx.conf
COPY Certificates/livehealthchecks.ui.crt /etc/nginx/livehealthchecks.ui.crt
COPY Certificates/livehealthchecks.ui.key /etc/nginx/livehealthchecks.ui.key
COPY Certificates/livehealthchecks.ui.pem /etc/nginx/livehealthchecks.ui.pem
RUN chmod 754 /etc/nginx/livehealthchecks.ui.crt
RUN chmod 754 /etc/nginx/livehealthchecks.ui.key
RUN chmod 754 /etc/nginx/livehealthchecks.ui.pem

EXPOSE 80
EXPOSE 443