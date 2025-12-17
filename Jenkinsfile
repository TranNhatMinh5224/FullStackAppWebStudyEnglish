pipeline {
    agent any

    // trigger sau moi 2 phut de kiem tra thay doi tren SCM 
    // triggers {
    //     pollSCM('H/2 * * * *')  
    // }

    environment {
        DOTNET_CLI_HOME = '/tmp/.dotnet'
        REGISTRY = "host.docker.internal:5000"
        IMAGE_NAME = "learningenglish-api"
        IMAGE_TAG = "${env.BUILD_NUMBER}-${env.GIT_COMMIT?.take(7) ?: 'latest'}"
        IMAGE_TAG_LATEST = "latest"
        FULL_IMAGE = "${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
        FULL_IMAGE_LATEST = "${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG_LATEST}"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                script {
                    env.GIT_COMMIT = sh(returnStdout: true, script: 'git rev-parse HEAD').trim()
                    echo "Building commit: ${env.GIT_COMMIT}"
                }
            }
        }

        stage('Restore') {
            steps {
                dir('BackendASP') {
                    sh 'dotnet restore'
                }
            }
        }

        stage('Build') {
            steps {
                dir('BackendASP') {
                    sh 'dotnet build -c Release --no-restore'
                }
            }
        }

        stage('Test') {
            steps {
                dir('BackendASP') {
                    sh '''
                    echo "Running unit tests..."
                    dotnet test LearningEnglish.Tests/LearningEnglish.Tests.csproj \
                      -c Release \
                      --no-build \
                      --logger trx \
                      --results-directory TestResults \
                      --verbosity normal
                    '''
                }
            }
        }

        stage('Publish') {
            steps {
                dir('BackendASP') {
                    sh '''
                    dotnet publish LearningEnglish.API/LearningEnglish.API.csproj \
                      -c Release \
                      -o publish
                    '''
                }
            }
        }

        stage('Docker Build') {
            steps {
                dir('BackendASP') {
                    sh '''
                    docker build -t ${FULL_IMAGE} -t ${FULL_IMAGE_LATEST} .
                    '''
                }
            }
        }

        stage('Docker Push') {
            steps {
                sh '''
                docker push ${FULL_IMAGE}
                docker push ${FULL_IMAGE_LATEST}
                echo "Pushed images: ${FULL_IMAGE} and ${FULL_IMAGE_LATEST}"
                '''
            }
        }

        stage('Deploy') {
            steps {
                dir('BackendASP') {
                    sh '''
                    docker compose down || true
                    docker compose pull
                    docker compose up -d
                    echo "Application deployed successfully!"
                    docker ps
                    '''
                }
            }
        }
    }

    post {
        always {
            dir('BackendASP') {
                archiveArtifacts artifacts: 'TestResults/**/*.trx', allowEmptyArchive: true
            }
        }
        success {
            echo ' CI/CD pipeline SUCCESS'
            echo "Image: ${FULL_IMAGE}"
        }
        failure {
            echo ' CI/CD pipeline FAILED'
        }
        cleanup {
            cleanWs()
        }
    }
}
