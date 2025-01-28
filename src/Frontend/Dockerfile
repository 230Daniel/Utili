FROM --platform=$BUILDPLATFORM node:22.7.0 as build
WORKDIR /app
COPY package.json ./
RUN npm install
COPY . ./
RUN npm run build

FROM nginx
COPY --from=build /app/build /build
CMD ["nginx", "-g", "daemon off;"]
