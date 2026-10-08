# Static files are the same for every architecture, so Node always runs on the build machine's platform.
FROM --platform=$BUILDPLATFORM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

# `docker compose --profile test run --rm web-tests`
FROM deps AS test
COPY . .
CMD ["sh", "-c", "npm run typecheck && npm test"]

FROM deps AS build
COPY . .
RUN npx ng build

FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/qanda-web/browser /usr/share/nginx/html
