// khai báo Jenkins Pipeline
pipeline {
    // sử dụng bất kỳ agent nào có sẵn
    agent any
    // thiết lập biến môi trường

    environment {
        
        REGISTRY_URL = 'localhost:5000' // URL của Docker registry
        REGISTRY_CREDENTIALS = 'docker-registry-credentials' // ID của credentials trong Jenkins
        IMAGE_NAME = 'learning-english-api' // tên của Docker image
        BACKEND_PATH = 'BackendASP' // đường dẫn đến thư mục backend

        NORMALIZED_BRANCH = "${env.BRANCH_NAME}".replaceAll('/', '-') // chuẩn hóa tên nhánh để sử dụng trong tag . ví dụ 'nfeature/xyz' thành 'feature-xyz'
        IMAGE_TAG = "${NORMALIZED_BRANCH}-${env.BUILD_NUMBER}" // tag của image dựa trên nhánh và số build . ví dụ 'feature-xyz-15'
        FULL_IMAGE_NAME = "${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}" // tên đầy đủ của image với tag . ví dụ 'localhost:5000/learning-english-api:feature-xyz-15'
        LATEST_IMAGE = "${REGISTRY_URL}/${IMAGE_NAME}:${NORMALIZED_BRANCH}-latest" // tên image với tag latest cho nhánh . ví dụ 'localhost:5000/learning-english-api:feature-xyz-latest'
    }
    // định nghĩa các giai đoạn của pipeline
    // mỗi giai đoạn đại diện cho một bước trong quy trình CI/CD

    stages { // bắt đầu định nghĩa các giai đoạn

        stage('Checkout') { // giai đoạn kiểm tra mã nguồn từ hệ thống quản lý phiên bản
            steps {  // bước thực hiện trong giai đoạn này
                checkout scm
                echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
                echo "Branch: ${env.BRANCH_NAME}"
                echo "Build: #${env.BUILD_NUMBER}"
                echo "Image Tag: ${IMAGE_TAG}"
                echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
            }
        }
        // giai đoạn xây dựng Docker image

        stage('Build Docker Image') {
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        docker build -t ${FULL_IMAGE_NAME} -t ${LATEST_IMAGE} .
                    '''
                }
            }
        }
        // giai đoạn đẩy Docker image lên registry 

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
        // giai đoạn triển khai ứng dụng dựa trên nhánh hiện tại

        stage('Deploy DEV') {
            when { branch 'dev' }
            steps {
                dir(BACKEND_PATH) {
                    sh '''
                        # Copy .env.example to .env.dev (will use actual values from file)
                        if [ -f .env.dev ]; then
                            echo ".env.dev already exists, using it"
                        else
                            echo "Creating .env.dev from .env.example"
                            cp .env.example .env.dev
                        fi
                        
                        # Deploy
                        docker compose -f docker-compose.dev.yml pull
                        docker compose -f docker-compose.dev.yml down || true
                        docker compose -f docker-compose.dev.yml up -d
                        
                        echo "✓ Deployment completed"
                        docker compose -f docker-compose.dev.yml ps
                    '''
                }
            }
        }
        // giai đoạn triển khai lên môi trường staging

        stage('Deploy STAGING') {
            when { branch 'staging' }
            steps {
                dir(BACKEND_PATH) {
                    withCredentials([
                        string(credentialsId: 'staging-postgres-password', variable: 'STAGING_DB_PASS'),
                        string(credentialsId: 'staging-jwt-key', variable: 'STAGING_JWT_KEY'),
                        string(credentialsId: 'staging-smtp-password', variable: 'STAGING_SMTP_PASS'),
                        string(credentialsId: 'staging-azure-speech-key', variable: 'STAGING_AZURE_SPEECH_KEY'),
                        string(credentialsId: 'staging-minio-access-key', variable: 'STAGING_MINIO_ACCESS_KEY'),
                        string(credentialsId: 'staging-minio-secret-key', variable: 'STAGING_MINIO_SECRET_KEY'),
                        string(credentialsId: 'staging-oxford-app-id', variable: 'STAGING_OXFORD_APP_ID'),
                        string(credentialsId: 'staging-oxford-app-key', variable: 'STAGING_OXFORD_APP_KEY'),
                        string(credentialsId: 'staging-unsplash-app-id', variable: 'STAGING_UNSPLASH_APP_ID'),
                        string(credentialsId: 'staging-unsplash-access-key', variable: 'STAGING_UNSPLASH_ACCESS_KEY'),
                        string(credentialsId: 'staging-unsplash-secret-key', variable: 'STAGING_UNSPLASH_SECRET_KEY'),
                        string(credentialsId: 'staging-google-client-id', variable: 'STAGING_GOOGLE_CLIENT_ID'),
                        string(credentialsId: 'staging-google-client-secret', variable: 'STAGING_GOOGLE_CLIENT_SECRET'),
                        string(credentialsId: 'staging-facebook-app-id', variable: 'STAGING_FACEBOOK_APP_ID'),
                        string(credentialsId: 'staging-facebook-app-secret', variable: 'STAGING_FACEBOOK_APP_SECRET'),
                        string(credentialsId: 'staging-casso-client-id', variable: 'STAGING_CASSO_CLIENT_ID'),
                        string(credentialsId: 'staging-casso-api-key', variable: 'STAGING_CASSO_API_KEY'),
                        string(credentialsId: 'staging-casso-checksum-key', variable: 'STAGING_CASSO_CHECKSUM_KEY'),
                        string(credentialsId: 'staging-payos-client-id', variable: 'STAGING_PAYOS_CLIENT_ID'),
                        string(credentialsId: 'staging-payos-api-key', variable: 'STAGING_PAYOS_API_KEY'),
                        string(credentialsId: 'staging-payos-checksum-key', variable: 'STAGING_PAYOS_CHECKSUM_KEY'),
                        string(credentialsId: 'staging-gemini-api-key', variable: 'STAGING_GEMINI_API_KEY')
                    ]) {
                        sh '''
                            # Generate .env.staging from .env.example with real secrets
                            cp .env.example .env.staging
                            sed -i "s/YOUR_POSTGRES_PASSWORD/${STAGING_DB_PASS}/g" .env.staging
                            sed -i "s/YOUR_PASSWORD/${STAGING_DB_PASS}/g" .env.staging
                            sed -i "s/YOUR_SECRET_KEY_HERE_MIN_32_CHARS/${STAGING_JWT_KEY}/g" .env.staging
                            sed -i "s/YOUR_APP_PASSWORD/${STAGING_SMTP_PASS}/g" .env.staging
                            sed -i "s/YOUR_AZURE_SPEECH_KEY/${STAGING_AZURE_SPEECH_KEY}/g" .env.staging
                            sed -i "s/YOUR_MINIO_ACCESS_KEY/${STAGING_MINIO_ACCESS_KEY}/g" .env.staging
                            sed -i "s/YOUR_MINIO_SECRET_KEY/${STAGING_MINIO_SECRET_KEY}/g" .env.staging
                            sed -i "s/YOUR_OXFORD_APP_ID/${STAGING_OXFORD_APP_ID}/g" .env.staging
                            sed -i "s/YOUR_OXFORD_APP_KEY/${STAGING_OXFORD_APP_KEY}/g" .env.staging
                            sed -i "s/YOUR_UNSPLASH_APP_ID/${STAGING_UNSPLASH_APP_ID}/g" .env.staging
                            sed -i "s/YOUR_UNSPLASH_ACCESS_KEY/${STAGING_UNSPLASH_ACCESS_KEY}/g" .env.staging
                            sed -i "s/YOUR_UNSPLASH_SECRET_KEY/${STAGING_UNSPLASH_SECRET_KEY}/g" .env.staging
                            sed -i "s/YOUR_GOOGLE_CLIENT_ID/${STAGING_GOOGLE_CLIENT_ID}/g" .env.staging
                            sed -i "s/YOUR_GOOGLE_CLIENT_SECRET/${STAGING_GOOGLE_CLIENT_SECRET}/g" .env.staging
                            sed -i "s/YOUR_FACEBOOK_APP_ID/${STAGING_FACEBOOK_APP_ID}/g" .env.staging
                            sed -i "s/YOUR_FACEBOOK_APP_SECRET/${STAGING_FACEBOOK_APP_SECRET}/g" .env.staging
                            sed -i "s/YOUR_CASSO_CLIENT_ID/${STAGING_CASSO_CLIENT_ID}/g" .env.staging
                            sed -i "s/YOUR_CASSO_API_KEY/${STAGING_CASSO_API_KEY}/g" .env.staging
                            sed -i "s/YOUR_CASSO_CHECKSUM_KEY/${STAGING_CASSO_CHECKSUM_KEY}/g" .env.staging
                            sed -i "s/YOUR_PAYOS_CLIENT_ID/${STAGING_PAYOS_CLIENT_ID}/g" .env.staging
                            sed -i "s/YOUR_PAYOS_API_KEY/${STAGING_PAYOS_API_KEY}/g" .env.staging
                            sed -i "s/YOUR_PAYOS_CHECKSUM_KEY/${STAGING_PAYOS_CHECKSUM_KEY}/g" .env.staging
                            sed -i "s/YOUR_GEMINI_API_KEY/${STAGING_GEMINI_API_KEY}/g" .env.staging
                            
                            # Deploy
                            docker compose -f docker-compose.staging.yml pull
                            docker compose -f docker-compose.staging.yml down || true
                            docker compose -f docker-compose.staging.yml up -d
                            
                            echo "✓ Staging deployment completed"
                            docker compose -f docker-compose.staging.yml ps
                        '''
                    }
                }
            }
        }
        // giai đoạn triển khai lên môi trường production

        stage('Deploy PROD') {
            when { branch 'main' }
            steps {
                input message: 'Deploy production?', ok: 'Deploy'
                dir(BACKEND_PATH) {
                    withCredentials([
                        string(credentialsId: 'prod-postgres-password', variable: 'PROD_DB_PASS'),
                        string(credentialsId: 'prod-jwt-key', variable: 'PROD_JWT_KEY'),
                        string(credentialsId: 'prod-smtp-password', variable: 'PROD_SMTP_PASS'),
                        string(credentialsId: 'prod-azure-speech-key', variable: 'PROD_AZURE_SPEECH_KEY'),
                        string(credentialsId: 'prod-minio-access-key', variable: 'PROD_MINIO_ACCESS_KEY'),
                        string(credentialsId: 'prod-minio-secret-key', variable: 'PROD_MINIO_SECRET_KEY'),
                        string(credentialsId: 'prod-oxford-app-id', variable: 'PROD_OXFORD_APP_ID'),
                        string(credentialsId: 'prod-oxford-app-key', variable: 'PROD_OXFORD_APP_KEY'),
                        string(credentialsId: 'prod-unsplash-app-id', variable: 'PROD_UNSPLASH_APP_ID'),
                        string(credentialsId: 'prod-unsplash-access-key', variable: 'PROD_UNSPLASH_ACCESS_KEY'),
                        string(credentialsId: 'prod-unsplash-secret-key', variable: 'PROD_UNSPLASH_SECRET_KEY'),
                        string(credentialsId: 'prod-google-client-id', variable: 'PROD_GOOGLE_CLIENT_ID'),
                        string(credentialsId: 'prod-google-client-secret', variable: 'PROD_GOOGLE_CLIENT_SECRET'),
                        string(credentialsId: 'prod-facebook-app-id', variable: 'PROD_FACEBOOK_APP_ID'),
                        string(credentialsId: 'prod-facebook-app-secret', variable: 'PROD_FACEBOOK_APP_SECRET'),
                        string(credentialsId: 'prod-casso-client-id', variable: 'PROD_CASSO_CLIENT_ID'),
                        string(credentialsId: 'prod-casso-api-key', variable: 'PROD_CASSO_API_KEY'),
                        string(credentialsId: 'prod-casso-checksum-key', variable: 'PROD_CASSO_CHECKSUM_KEY'),
                        string(credentialsId: 'prod-payos-client-id', variable: 'PROD_PAYOS_CLIENT_ID'),
                        string(credentialsId: 'prod-payos-api-key', variable: 'PROD_PAYOS_API_KEY'),
                        string(credentialsId: 'prod-payos-checksum-key', variable: 'PROD_PAYOS_CHECKSUM_KEY'),
                        string(credentialsId: 'prod-gemini-api-key', variable: 'PROD_GEMINI_API_KEY')
                    ]) {
                        sh '''
                            # Generate .env.prod from .env.example with real secrets
                            cp .env.example .env.prod
                            sed -i "s/YOUR_POSTGRES_PASSWORD/${PROD_DB_PASS}/g" .env.prod
                            sed -i "s/YOUR_PASSWORD/${PROD_DB_PASS}/g" .env.prod
                            sed -i "s/YOUR_SECRET_KEY_HERE_MIN_32_CHARS/${PROD_JWT_KEY}/g" .env.prod
                            sed -i "s/YOUR_APP_PASSWORD/${PROD_SMTP_PASS}/g" .env.prod
                            sed -i "s/YOUR_AZURE_SPEECH_KEY/${PROD_AZURE_SPEECH_KEY}/g" .env.prod
                            sed -i "s/YOUR_MINIO_ACCESS_KEY/${PROD_MINIO_ACCESS_KEY}/g" .env.prod
                            sed -i "s/YOUR_MINIO_SECRET_KEY/${PROD_MINIO_SECRET_KEY}/g" .env.prod
                            sed -i "s/YOUR_OXFORD_APP_ID/${PROD_OXFORD_APP_ID}/g" .env.prod
                            sed -i "s/YOUR_OXFORD_APP_KEY/${PROD_OXFORD_APP_KEY}/g" .env.prod
                            sed -i "s/YOUR_UNSPLASH_APP_ID/${PROD_UNSPLASH_APP_ID}/g" .env.prod
                            sed -i "s/YOUR_UNSPLASH_ACCESS_KEY/${PROD_UNSPLASH_ACCESS_KEY}/g" .env.prod
                            sed -i "s/YOUR_UNSPLASH_SECRET_KEY/${PROD_UNSPLASH_SECRET_KEY}/g" .env.prod
                            sed -i "s/YOUR_GOOGLE_CLIENT_ID/${PROD_GOOGLE_CLIENT_ID}/g" .env.prod
                            sed -i "s/YOUR_GOOGLE_CLIENT_SECRET/${PROD_GOOGLE_CLIENT_SECRET}/g" .env.prod
                            sed -i "s/YOUR_FACEBOOK_APP_ID/${PROD_FACEBOOK_APP_ID}/g" .env.prod
                            sed -i "s/YOUR_FACEBOOK_APP_SECRET/${PROD_FACEBOOK_APP_SECRET}/g" .env.prod
                            sed -i "s/YOUR_CASSO_CLIENT_ID/${PROD_CASSO_CLIENT_ID}/g" .env.prod
                            sed -i "s/YOUR_CASSO_API_KEY/${PROD_CASSO_API_KEY}/g" .env.prod
                            sed -i "s/YOUR_CASSO_CHECKSUM_KEY/${PROD_CASSO_CHECKSUM_KEY}/g" .env.prod
                            sed -i "s/YOUR_PAYOS_CLIENT_ID/${PROD_PAYOS_CLIENT_ID}/g" .env.prod
                            sed -i "s/YOUR_PAYOS_API_KEY/${PROD_PAYOS_API_KEY}/g" .env.prod
                            sed -i "s/YOUR_PAYOS_CHECKSUM_KEY/${PROD_PAYOS_CHECKSUM_KEY}/g" .env.prod
                            sed -i "s/YOUR_GEMINI_API_KEY/${PROD_GEMINI_API_KEY}/g" .env.prod
                            
                            # Deploy
                            docker compose -f docker-compose.prod.yml pull
                            docker compose -f docker-compose.prod.yml down || true
                            docker compose -f docker-compose.prod.yml up -d
                            
                            echo "✓ Production deployment completed"
                            docker compose -f docker-compose.prod.yml ps
                        '''
                    }
                }
            }
        }
    }
    // khối post để xử lý kết quả của pipeline

    post {
        success {
            echo "✅ Pipeline completed successfully"
        }
        failure {
            echo "❌ Pipeline failed"
        }
    }
}
