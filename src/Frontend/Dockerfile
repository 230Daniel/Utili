FROM node:16.17.0 as build-deps
WORKDIR /app
COPY package.json yarn.lock ./
RUN yarn
COPY . ./
RUN yarn build

FROM nginx:1.12-alpine
COPY --from=build-deps /app/build /build
CMD ["nginx", "-g", "daemon off;"]
