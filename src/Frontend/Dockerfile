FROM node:22.7.0 as build-deps
WORKDIR /app
COPY package.json ./
RUN npm install
COPY . ./
RUN npm run build

FROM nginx
COPY --from=build-deps /app/build /build
CMD ["nginx", "-g", "daemon off;"]
