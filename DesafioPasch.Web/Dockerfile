# ====== Build Angular ======
FROM node:20-alpine AS build

WORKDIR /app

# Dependências
COPY package*.json ./
RUN npm ci

# Código
COPY . .

# Build (ajuste se seu script for diferente)
RUN npm run build

# ====== Nginx (serve + proxy /api) ======
FROM nginx:1.27-alpine

# Remove config default
RUN rm -f /etc/nginx/conf.d/default.conf

# Copia config do nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copia build do Angular (ajuste o path se necessário)
# Angular (v17+) costuma gerar em dist/<project>/browser
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
