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
        stage( 'docker build'){
            steps{
                // This step should not normally be used in your script. Consult the inline help for details.
                withDockerRegistry(credentialsId: 'docker') {
                sh"docker image build -t nop:${BUILD_ID} ."
                sh"docker image tag nop:${BUILD_ID}  nazziops/project:${BUILD_ID}"
                sh"docker image push nazziops/project:${BUILD_ID}"
            }
        }
        stage('terraform'){
            steps{
                sh "cd ./deploy/terraform"
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
}
