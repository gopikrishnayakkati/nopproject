pipeline{
    agent any
    triggers{
        pollSCM ( '* * * * *')
    }
    stages{
        stage( 'VCS'){
            steps{
                git url: 'https://github.com/Gopi0527/nopproject.git',
                branch: 'develop'
                }
            }
        stage( 'Build the dotnet'){
            steps{
                sh 'dotnet build -c Release src/NopCommerce.sln'
                sh 'dotnet publish -c Release src/Presentation/Nop.Web/Nop.Web.csproj  -o "./published"'
                }
            
        }
        stage ('docker build') {
            steps {
               script {
                withDockerRegistry(credentialsId: 'docker', toolName: 'docker') {
                sh"docker image build -t nop:${BUILD_ID} ."
                sh"docker image tag nop:${BUILD_ID}  gopikrishna0527/nopproj:${BUILD_ID}"
                sh"docker image push gopikrishna0527/nopproj:${BUILD_ID}"
                }
            }
            
           
            }
        }
        stage('terraform'){
            steps{
                sh "https://github.com/Gopi0527/nopproject.git"
                sh "cd nopproject\deploy\terraform"
                sh "terraform init && terraform apply -auto-approve"
            }
        }
        stage ('deploy in k8s'){
            steps{
                sh "kubectl apply -f ./deploy/k8s"
            }
        
                
          }
     }


    }

