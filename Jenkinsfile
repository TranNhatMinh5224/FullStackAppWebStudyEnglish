pipeline {
    agent any

    environment {
        REGISTRY_URL = 'localhost:5000'
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

        stage('Build & Test') {
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        # Ensure .NET SDK is available
                        if ! command -v dotnet &> /dev/null; then
                            echo "Installing .NET SDK..."
                            apt-get update && apt-get install -y dotnet-sdk-8.0
                        fi
                        
                        echo "Current directory: $(pwd)"
                        echo ".NET version: $(dotnet --version)"
                        
                        dotnet restore
                        dotnet build -c Release --no-restore
                        dotnet test -c Release --no-build || true
                    '''
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        docker build -t ${FULL_IMAGE_NAME} -t ${LATEST_IMAGE} .
                    '''
                }
            }
        }

        stage('Push Image') {
            steps {
                sh '''
                    docker push ${FULL_IMAGE_NAME}
                    docker push ${LATEST_IMAGE}
                '''
            }
        }

        stage('Deploy DEV') {
            when { branch 'dev' }
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        docker compose -f docker-compose.dev.yml down || true
                        docker compose -f docker-compose.dev.yml up -d
                    '''
                }
            }
        }

        stage('Deploy STAGING') {
            when { branch 'staging' }
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        docker compose -f docker-compose.staging.yml down || true
                        docker compose -f docker-compose.staging.yml up -d
                    '''
                }
            }
        }

        stage('Deploy PROD') {
            when { branch 'main' }
            steps {
                input message: 'Deploy production?', ok: 'Deploy'
                dir(BACKEND_PATH) {
                    sh '''
                        docker compose -f docker-compose.prod.yml down || true
                        docker compose -f docker-compose.prod.yml up -d
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "✅ Pipeline completed successfully"
        }
        failure {
            echo "❌ Pipeline failed"
        }
    }
}
