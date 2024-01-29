# FunctionAppFiapF2

FunctionAppFiapF2 é um projeto desenvolvido como proposta para entrega do Tech Challenge Fase 2.

## Enunciado do Tech Challenge

Deve desenvolver uma Function utilizando o template durable. Ela deve simular o processo de aprovação de um pedido. O formato está livre, mas vocês devem criar no formato durable para colocar este modelo em prática. 

## Aplicação do projeto

A durable simula a aprovação de uma solicitação de um pedido. Onde ao ser aprovado, o pedido é processado e gerado uma identificação para o pedido.

## Desenvolvimento 

Para rodar esse projeto você pode usar o Visual Studio. E seguir os passos abaixo:

* Abra o projeto no Visual Studio.
* Abra o Package Manager Console, execute o comando Update-Database
* Após o término da execução do comando, rode o projeto.

### Banco de Dados

Esse projeto está usando SQL Server, você pode utilizar uma instância que tem instalado na sua máquina.

### Durable Function

A Durable Function foi criada com base no pattern Function Chaining. E foi desenvolvida para receber um JSON via post.

# JSON:

JSON para solicitação via post:
````````````
{
    "CodigoProduto": "P002",
    "NumeroCartaoCredito": "1234567890123456",
    "Quantidade": "8",
    "NomeCliente": "FIAP"
}
``````````````
### Projeto

O projeto foi construído com a linguagem C#, AspNet 7.0.

### Observções

Ao executar o projeto o bando de dados será populado com os registros:

Produto:

| Codigo   |      Nome      |
|----------|:-------------:|
| P001 |  Camisa | 
| P002 | Calça   |   
| P003 | Meia |    
| P004 | Luva |  



Estoque

 CodigoProduto = "P001", Quantidade = 10;
 CodigoProduto = "P002", Quantidade = 10;
 CodigoProduto = "P003", Quantidade = 10;
 CodigoProduto = "P004", Quantidade = 10;


## Grupo 33
Jorge - RM351049
