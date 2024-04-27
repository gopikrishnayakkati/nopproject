pipeline{
    agent any
    triggers{
        pollSCM ( '* * * * *')
    }
    stages{
        stage( 'VCS'){
            steps{
                git url: 'https://github.com/Gopi0527/nopCommerce.git',
                branch: 'develop'
                }
            }
        stage( 'Build the dotnet'){
            steps{
                sh 'dotnet build -c Release src/NopCommerce.sln'
                sh 'dotnet publish -c Release src/Presentation/Nop.Web/Nop.Web.csproj  -o "./published"'
                }
            post{
                success{
                    sh 'mkdir ./published/bin ./published/logs'
                    sh 'tar -czvf nop.web.tar.gz ./published/'
                    archiveArtifacts  artifacts: '**/*.tar.gz'
                     }
                } 
        }
        stage( 'docker build'){
            steps{
                sh"docker image build -t nop:1.0 ."
                sh"docker image tag nop:1.0  <docker repo name>"
                sh"docker image push <repo name>"
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
