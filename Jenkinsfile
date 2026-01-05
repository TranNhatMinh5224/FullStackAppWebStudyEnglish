// Jenkinsfile ĐƠN GIẢN cho Cloudflare Tunnel
// Branch flow: feature/* → dev → staging → main
//
// STRATEGY: Local deployment với Cloudflare Tunnel
// - Feature: CI only (build, test, push image)
// - Dev: CI + Auto deploy local
// - Staging: CI + Auto deploy local
// - Main: CI + Manual approval + deploy local
//

pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:8.0'
            args '-v /var/run/docker.sock:/var/run/docker.sock'
        }
    }

    environment {
        REGISTRY_URL = 'host.docker.internal:5000'
        REGISTRY_CREDENTIALS = 'docker-registry-credentials'
        IMAGE_NAME = 'learning-english-api'
        BACKEND_PATH = 'BackendASP'

        NORMALIZED_BRANCH = "${env.BRANCH_NAME}".replaceAll('/', '-')
        IMAGE_TAG = "${NORMALIZED_BRANCH}-${env.BUILD_NUMBER}"
        FULL_IMAGE_NAME = "${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}"
        LATEST_IMAGE = "${REGISTRY_URL}/${IMAGE_NAME}:${NORMALIZED_BRANCH}-latest"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo "Branch: ${env.BRANCH_NAME}"
            }
        }

        stage('Setup Environment') {
            steps {
                sh '''
                    echo "Installing Docker CLI..."
                    apt-get update && apt-get install -y docker.io
                    docker --version
                    dotnet --version
                    echo "Environment ready!"
                '''
            }
        }

        stage('Environment Info') {
            steps {
                script {
                    def willDeploy = env.BRANCH_NAME in ['dev', 'staging', 'main']
                    sh """
                        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
                        echo "Branch: ${BRANCH_NAME}"
                        echo "Build: #${BUILD_NUMBER}"
                        echo "Image: ${IMAGE_TAG}"
                        echo "Deploy: ${willDeploy ? 'YES' : 'NO'}"
                        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
                        docker --version
                        dotnet --version
                    """
                }
            }
        }

        stage('Build & Test') {
            steps {
                dir(BACKEND_PATH) {
                    sh """
                        echo "Building .NET project..."
                        dotnet restore
                        dotnet build --configuration Release --no-restore
                        echo "Running tests..."
                        dotnet test --configuration Release --no-build --verbosity normal
                    """
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                dir(BACKEND_PATH) {
                    sh """
                        echo "Building Docker image: ${FULL_IMAGE_NAME}"
                        docker build -t ${FULL_IMAGE_NAME} -t ${LATEST_IMAGE} .
                    """
                }
            }
        }

        stage('Push to Registry') {
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: REGISTRY_CREDENTIALS, usernameVariable: 'REGISTRY_USER', passwordVariable: 'REGISTRY_PASS')]) {
                        sh """
                            echo "Logging into registry..."
                            echo "${REGISTRY_PASS}" | docker login ${REGISTRY_URL} -u "${REGISTRY_USER}" --password-stdin
                            echo "Pushing image..."
                            docker push ${FULL_IMAGE_NAME}
                            docker push ${LATEST_IMAGE}
                            echo "Image pushed successfully"
                        """
                    }
                }
            }
        }

        stage('Deploy Dev') {
            when {
                branch 'dev'
            }
            steps {
                script {
                    deployLocal('dev')
                }
            }
        }

        stage('Deploy Staging') {
            when {
                branch 'staging'
            }
            steps {
                script {
                    deployLocal('staging')
                }
            }
        }

        stage('Deploy Production') {
            when {
                branch 'main'
            }
            steps {
                script {
                    input message: 'Deploy to Production?', ok: 'Deploy'
                    deployLocal('prod')
                }
            }
        }
    }

    post {
        always {
            sh """
                echo "Cleaning up..."
                docker system prune -f
                docker image prune -f
            """
        }
        success {
            echo "Pipeline succeeded!"
        }
        failure {
            echo "Pipeline failed!"
        }
    }
}

def deployLocal(String env) {
    def composeFile = "docker-compose.${env}.yml"
    def envFile = ".env.${env}"

    sh """
        echo "Deploying to ${env} environment..."
        echo "Using compose file: ${composeFile}"
        echo "Using env file: ${envFile}"

        # Stop existing containers
        docker compose -f ${composeFile} --env-file ${envFile} down || true

        # Update image in compose file (if needed)
        # For simplicity, assume compose file uses latest tag

        # Start new containers
        docker compose -f ${composeFile} --env-file ${envFile} up -d

        # Wait for health check
        echo "Waiting for services to be healthy..."
        sleep 30

        # Check container status
        docker compose -f ${composeFile} --env-file ${envFile} ps

        echo "Deployment to ${env} completed!"
    """
}
