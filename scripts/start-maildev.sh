#!/bin/bash

# ============================================================================
# MailDev Docker Container Startup Script
# ============================================================================
# Description: Starts MailDev SMTP server for development environment.
# MailDev captures all outgoing emails and provides a web UI to view them.
#
# Usage:
#   ./scripts/start-maildev.sh [options]
#
# Options:
#   --stop      Stop and remove the MailDev container
#   --restart   Restart the MailDev container
#   --logs      Show container logs
#   --status    Show container status
#   --help      Show this help message
#
# Web UI: http://localhost:1080
# SMTP Port: 1025
#
# Reference: https://hub.docker.com/r/maildev/maildev
# ============================================================================

set -e

CONTAINER_NAME="medicalcenter-maildev"
SMTP_PORT=1025
WEB_PORT=1080
IMAGE="maildev/maildev"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  MailDev - Development SMTP${NC}"
    echo -e "${BLUE}================================${NC}"
}

show_help() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  (none)      Start MailDev container"
    echo "  --stop      Stop and remove the MailDev container"
    echo "  --restart   Restart the MailDev container"
    echo "  --logs      Show container logs (follow mode)"
    echo "  --status    Show container status"
    echo "  --help      Show this help message"
    echo ""
    echo "Ports:"
    echo "  SMTP:     localhost:${SMTP_PORT}"
    echo "  Web UI:   http://localhost:${WEB_PORT}"
    echo ""
    echo "Reference: https://hub.docker.com/r/maildev/maildev"
    exit 0
}

check_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH"
        echo "Please install Docker: https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running"
        echo "Please start Docker Desktop or the Docker daemon"
        exit 1
    fi
}

stop_container() {
    if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Stopping MailDev container..."
        docker stop ${CONTAINER_NAME} 2>/dev/null || true
        docker rm ${CONTAINER_NAME} 2>/dev/null || true
        print_info "MailDev container stopped and removed"
    else
        print_warn "MailDev container is not running"
    fi
}

show_logs() {
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Showing MailDev logs (Ctrl+C to exit)..."
        docker logs -f ${CONTAINER_NAME}
    else
        print_error "MailDev container is not running"
        exit 1
    fi
}

show_status() {
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "MailDev is running"
        echo ""
        docker ps --filter "name=${CONTAINER_NAME}" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
    else
        print_warn "MailDev is not running"
        echo "Run '$0' to start MailDev"
    fi
}

start_container() {
    check_docker
    print_header
    echo ""
    
    # Check if container already exists and is running
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "MailDev is already running"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
        echo ""
        print_info "To stop: $0 --stop"
        exit 0
    fi
    
    # Remove stopped container if exists
    if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Removing stopped MailDev container..."
        docker rm ${CONTAINER_NAME} 2>/dev/null || true
    fi
    
    print_info "Starting MailDev container..."
    print_info "Pulling latest image..."
    
    docker pull ${IMAGE} --quiet
    
    docker run -d \
        --name ${CONTAINER_NAME} \
        -p ${SMTP_PORT}:1025 \
        -p ${WEB_PORT}:1080 \
        --restart unless-stopped \
        ${IMAGE}
    
    # Wait for container to be ready
    print_info "Waiting for MailDev to be ready..."
    sleep 2
    
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        echo ""
        print_info "MailDev is now running!"
        echo ""
        echo "  📧 SMTP Server:  localhost:${SMTP_PORT}"
        echo "  🌐 Web UI:       http://localhost:${WEB_PORT}"
        echo ""
        echo "  Configure your application with:"
        echo "    Host:       localhost"
        echo "    Port:       ${SMTP_PORT}"
        echo "    SSL:        false"
        echo "    UseMailDev: true"
        echo ""
        print_info "View captured emails at: http://localhost:${WEB_PORT}"
        print_info "To stop: $0 --stop"
    else
        print_error "Failed to start MailDev container"
        echo "Check Docker logs: docker logs ${CONTAINER_NAME}"
        exit 1
    fi
}

# Main script
case "${1:-}" in
    --stop)
        stop_container
        ;;
    --restart)
        stop_container
        start_container
        ;;
    --logs)
        show_logs
        ;;
    --status)
        show_status
        ;;
    --help|-h)
        show_help
        ;;
    "")
        start_container
        ;;
    *)
        print_error "Unknown option: $1"
        echo ""
        show_help
        ;;
esac

