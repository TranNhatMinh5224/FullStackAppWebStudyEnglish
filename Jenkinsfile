pipeline {
    agent any

    environment {
        REGISTRY_URL = 'localhost:5000'
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
                echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
                echo "Branch: ${env.BRANCH_NAME}"
                echo "Build: #${env.BUILD_NUMBER}"
                echo "Image Tag: ${IMAGE_TAG}"
                echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
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
                script {
                    withCredentials([usernamePassword(credentialsId: REGISTRY_CREDENTIALS, usernameVariable: 'REGISTRY_USER', passwordVariable: 'REGISTRY_PASS')]) {
                        sh '''
                            echo "Logging into registry..."
                            echo "${REGISTRY_PASS}" | docker login ${REGISTRY_URL} -u "${REGISTRY_USER}" --password-stdin
                            echo "Pushing images..."
                            docker push ${FULL_IMAGE_NAME}
                            docker push ${LATEST_IMAGE}
                            echo "✓ Images pushed successfully"
                        '''
                    }
                }
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
