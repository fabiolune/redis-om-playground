# Build stage
FROM node:23.9-slim AS build

WORKDIR /app

# Copy package files first for better layer caching
COPY redis.om.playground.ui/package*.json ./

# Install all dependencies (including devDependencies for build)
RUN --mount=type=cache,target=/root/.npm \
    npm ci --cache /root/.npm

# Copy source code
COPY redis.om.playground.ui/ ./

# Build the application
RUN npm run build

# # Runtime stage
# FROM node:20-alpine AS runtime

# WORKDIR /app

# # Copy package files
# COPY redis.om.playground.ui/package*.json ./

# # Install only production dependencies
# RUN --mount=type=cache,target=/root/.npm \
#     npm ci --only=production --cache /root/.npm && \
#     npm install -g serve

# # Copy built application from build stage
# COPY --from=build /app/dist ./dist

# # Create non-root user
# RUN addgroup -g 1001 -S nodejs && \
#     adduser -S nextjs -u 1001 && \
#     chown -R nextjs:nodejs /app

# USER nextjs

# # Expose port
# EXPOSE 3000

# # Serve the application
# CMD ["serve", "-s", "dist", "-l", "3000"]