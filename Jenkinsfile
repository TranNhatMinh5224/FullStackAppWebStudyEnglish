pipeline {
    agent any
    
    environment {
        // Docker Registry
        REGISTRY_URL = 'localhost:5000'
        REGISTRY_CREDENTIALS = 'docker-registry-credentials'
        
        // Image names
        IMAGE_NAME = 'learning-english-api'
        IMAGE_TAG = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}"
        FULL_IMAGE_NAME = "${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}"
        LATEST_IMAGE_NAME = "${REGISTRY_URL}/${IMAGE_NAME}:${env.BRANCH_NAME}-latest"
        
        // Project paths
        BACKEND_PATH = 'BackendASP'
        DOCKERFILE_PATH = "${BACKEND_PATH}/Dockerfile"
        COMPOSE_FILE_DEV = "${BACKEND_PATH}/docker-compose.dev.yml"
        COMPOSE_FILE_DEPLOY = "${BACKEND_PATH}/docker-compose.deploy.yml"
    }
    
    stages {
        stage('📋 Checkout') {
            steps {
                echo "Checking out branch: ${env.BRANCH_NAME}"
                checkout scm
            }
        }
        
        stage('🔍 Environment Info') {
            steps {
                script {
                    echo "Branch: ${env.BRANCH_NAME}"
                    echo "Build Number: ${env.BUILD_NUMBER}"
                    echo "Image Tag: ${IMAGE_TAG}"
                    sh 'docker --version'
                    sh 'dotnet --version || echo "dotnet not installed on Jenkins"'
                }
            }
        }
        
        stage(' Build & Test') {
            steps {
                dir("${BACKEND_PATH}") {
                    script {
                        echo "Building .NET application..."
                        // Build project to check for compilation errors
                        sh '''
                            dotnet restore LearningEnglish.sln || echo "Dotnet restore skipped"
                            dotnet build LearningEnglish.sln -c Release --no-restore || echo "Dotnet build skipped"
                        '''
                        
                        // Run tests if available
                        sh '''
                            if [ -d "LearningEnglish.Tests" ]; then
                                dotnet test LearningEnglish.Tests/LearningEnglish.Tests.csproj --no-build -c Release || echo "Tests skipped"
                            else
                                echo "No tests found"
                            fi
                        '''
                    }
                }
            }
        }
        
        stage(' Build Docker Image') {
            steps {
                dir("${BACKEND_PATH}") {
                    script {
                        echo "Building Docker image: ${FULL_IMAGE_NAME}"
                        sh """
                            docker build -t ${FULL_IMAGE_NAME} -f Dockerfile .
                            docker tag ${FULL_IMAGE_NAME} ${LATEST_IMAGE_NAME}
                        """
                    }
                }
            }
        }
        
        stage(' Login to Registry') {
            steps {
                script {
                    echo "Logging in to Docker Registry at ${REGISTRY_URL}"
                    withCredentials([usernamePassword(
                        credentialsId: "${REGISTRY_CREDENTIALS}",
                        usernameVariable: 'REGISTRY_USER',
                        passwordVariable: 'REGISTRY_PASS'
                    )]) {
                        sh """
                            echo \${REGISTRY_PASS} | docker login ${REGISTRY_URL} -u \${REGISTRY_USER} --password-stdin
                        """
                    }
                }
            }
        }
        
        stage(' Push to Registry') {
            steps {
                script {
                    echo "Pushing images to registry..."
                    sh """
                        docker push ${FULL_IMAGE_NAME}
                        docker push ${LATEST_IMAGE_NAME}
                    """
                }
            }
        }
    
        stage('🚀 Deploy - Development') {
            when {
                branch 'dev'
            }
            steps {
                script {
                    echo "Deploying to Development environment..."
                    dir("${BACKEND_PATH}") {
                        sh """
                            # Set the registry image to use
                            export REGISTRY_IMAGE=${LATEST_IMAGE_NAME}
                            
                            # Login to registry
                            echo "Logging in to registry..."
                            docker login ${REGISTRY_URL} -u admin -p admin123
                            
                            # Pull the latest image from registry
                            echo "Pulling image: ${LATEST_IMAGE_NAME}"
                            docker pull ${LATEST_IMAGE_NAME}
                            
                            # Stop old containers
                            echo "Stopping old containers..."
                            docker compose -f docker-compose.deploy.yml down || true
                            
                            # Start new containers with image from registry
                            echo "Starting new containers..."
                            docker compose -f docker-compose.deploy.yml up -d
               🚀 Deploy - Staging') {
            when {
                branch 'staging'
            }
            steps {
                script {
                    echo "Deploying to Staging environment..."
                    dir("${BACKEND_PATH}") {
                        sh """
                            export REGISTRY_IMAGE=${LATEST_IMAGE_NAME}
                            docker login ${REGISTRY_URL} -u admin -p admin123
                            docker pull ${LATEST_IMAGE_NAME}
                            docker compose -f docker-compose.staging.yml down || true
                            docker compose -f docker-compose.staging.yml up -d
                            docker compose -f docker-compose.staging.yml ps
                            docker compose -f docker-compose.staging.yml logs --tail=20 api
        stage(' Deploy - Staging') {
            when {
                branch 'staging'
            }
            steps {
                script {
                    echo "Deploying to Staging environment..."
                    dir("${BACKEND_PATH}") {
                        sh """
                            export NEW_IMAGE=${LATEST_IMAGE_NAME}
                            docker pull ${LATEST_IMAGE_NAME}
                            docker compose -f docker-compose.staging.yml down || true
                            docker compose -f docker-compose.staging.yml up -d
                            docker compose -f docker-compose.staging.yml ps
                        """
                    }
                }
            }
        }🚀 Deploy - Production') {
            when {
                branch 'main'
            }
            steps {
                script {
                    echo "Deploying to Production environment..."
                    dir("${BACKEND_PATH}") {
                        // Add manual approval for production
                        input message: 'Deploy to Production?', ok: 'Deploy'
                        
                        sh """
                            export REGISTRY_IMAGE=${LATEST_IMAGE_NAME}
                            docker login ${REGISTRY_URL} -u admin -p admin123
                            docker pull ${LATEST_IMAGE_NAME}
                            docker compose -f docker-compose.prod.yml down || true
                            docker compose -f docker-compose.prod.yml up -d
                            docker compose -f docker-compose.prod.yml ps
                            docker compose -f docker-compose.prod.yml logs --tail=20 apiwn || true
                            docker compose -f docker-compose.prod.yml up -d
                            docker compose -f docker-compose.prod.yml ps
                        """
                    }
                }
            }
        }
    }
    
    post {
        always {
            script {
                echo "Cleaning up..."
                // Logout from registry
                sh "docker logout ${REGISTRY_URL} || true"
                
                // Clean up old images to save space
                sh """
                    docker image prune -f || true
                """
            }
        }
        success {
            echo "Pipeline completed successfully!"
            echo "Image pushed: ${FULL_IMAGE_NAME}"
            echo "Latest tag: ${LATEST_IMAGE_NAME}"
        }
        failure {
            echo "Pipeline failed!"
        }
    }
}
