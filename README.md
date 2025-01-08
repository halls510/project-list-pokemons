### README - Backend Challenge: Pokémons

## **Sumário**
1. [Descrição](#descrição)
2. [Tecnologias Utilizadas](#tecnologias-utilizadas)
3. [Funcionalidades](#funcionalidades)
4. [Pré-requisitos](#pré-requisitos)
5. [Variáveis de Ambiente](#variáveis-de-ambiente)
6. [Arquivos Importantes](#arquivos-importantes)
7. [Configuração com Docker Compose](#configuração-com-docker-compose)
8. [Configurações de `appsettings`](#configurações-de-appsettings)
9. [Gerenciamento de Logs](#gerenciamento-de-logs)
10. [Rodando o Projeto](#rodando-o-projeto)
11. [Testando a API](#testando-a-api)
12. [Estrutura do Projeto](#estrutura-do-projeto)
13. [Desafios e Soluções](#desafios-e-soluções)
14. [Evolução e Estratégias Futuras](#evolução-e-estratégias-futuras)
15. [Apresentação do Projeto](#apresentação-do-projeto)
15. [Links Úteis](#links-úteis)

---

## **Descrição**

Este projeto foi desenvolvido como parte de um desafio de Backend Developer para listar Pokémons utilizando a PokeAPI. Ele oferece funcionalidades de gerenciamento de Mestres Pokémon, captura de Pokémons e exibição de informações detalhadas sobre eles, mesmo quando a PokeAPI está indisponível, graças ao armazenamento local em SQLite e ao uso de cache Redis.

Além disso, foi implementado um sistema básico de autenticação JWT, com credenciais fixas:
- **Username:** `admin`
- **Password:** `123456789`

A autenticação é necessária para acessar os endpoints protegidos, garantindo um nível básico de segurança.

---

## **Tecnologias Utilizadas**

- **Linguagem:** C#
- **Framework:** .NET 8.0
- **Banco de Dados:** SQLite
- **Bibliotecas:** Entity Framework Core, System.Text.Json
- **API Externa:** PokeAPI
- **Cache:** Redis
- **Autenticação:** JWT (com credenciais fixas)
- **Ferramentas:** Visual Studio 2022, Docker, Postman, Swagger

---

## **Funcionalidades**

- Listar 10 Pokémons aleatórios com informações básicas (nome, sprites em formato Base64, etc.).
- Buscar informações de um Pokémon por ID.
- Cadastro de Mestres Pokémon com validação básica (nome, idade, CPF).
- Registro de captura de Pokémons pelos Mestres Pokémon.
- Listagem de Pokémons capturados por Mestre.
- Persistência local de dados: O sistema utiliza o banco de dados SQLite para armazenar informações de Pokémons, garantindo acesso offline e resiliência contra falhas na PokeAPI. Antes de realizar qualquer consulta à PokeAPI, o sistema verifica primeiro o cache Redis e, em seguida, o banco de dados local. Apenas se as informações não estiverem disponíveis nessas fontes, a PokeAPI será consultada. Além disso, o banco de dados local é sincronizado automaticamente com a PokeAPI a cada 24 horas, garantindo que os dados estejam sempre atualizados e minimizando o número de requisições à API externa.

---

## **Pré-requisitos**

- Docker instalado (obrigatório).

---

## **Variáveis de Ambiente**

| Variável                     | Valor               | Descrição                                                                 |
|------------------------------|---------------------|---------------------------------------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`     | `Development`/`Production` | Define o ambiente da aplicação.                                          |
| `ASPNETCORE_URLS`            | `http://+:5000`    | Define a URL base para a aplicação.                                      |
| `ConnectionStrings__SQLiteConnection` | Caminho SQLite    | Configuração para o banco de dados SQLite.                               |
| `RedisConnection`            | `redis:6379`       | Conexão para o Redis.                                                    |
| `RESET_DATABASE_SQLITE`      | `true` ou `false`  | Se `true`, recria o banco de dados no início da execução.                |
| `SYNC_INTERVAL_HOURS`        | Inteiro            | Intervalo (em horas) para sincronizar dados com a PokeAPI.               |

---

## **Arquivos Importantes**

### **Docker Compose**

- **Desenvolvimento:**
  - `docker-compose.yml`: Configuração básica para desenvolvimento.
  - `docker-compose.override.yml`: Configuração adicional para desenvolvimento.

- **Produção:**
  - `docker-compose.production.yml`: Configuração para ambiente de produção.

### **AppSettings**

- **Desenvolvimento:**
  - `appsettings.Development.json`: Configurações específicas para o ambiente de desenvolvimento.

- **Produção:**
  - `appsettings.json`: Configurações específicas para o ambiente de produção.

---

## **Configuração com Docker Compose**

### **1. Desenvolvimento**
Arquivo: `docker-compose.yml` e `docker-compose.override.yml`

```yaml
version: '3.8'

services:
  redis:
    image: redis:7.0
    container_name: redis
    ports:
      - "6379:6379"
    networks:
      - app_network
    command: ["redis-server", "--appendonly", "yes"]

  project-list-pokemons.api:
    build:
      context: .
      dockerfile: project-list-pokemons.Api/Dockerfile
    image: projectlistpokemonsapi
    container_name: project-list-pokemons
    ports:
      - "5000:5000"
    depends_on:
      - redis
    networks:
      - app_network
    volumes:
      - sqlite_data:/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__SQLiteConnection=Data Source=/data/pokemon_dev.db
      - RedisConnection=redis:6379
      - RESET_DATABASE_SQLITE=true
      - SYNC_INTERVAL_HOURS=1      

volumes:
  sqlite_data:
    driver: local

networks:
  app_network:
    driver: bridge
```

### **2. Produção**
Arquivo: `docker-compose.production.yml`

```yaml
version: '3.8'

services:
  redis:
    image: redis:7.0
    container_name: redis
    ports:
      - "6379:6379"
    networks:
      - app_network
    command: ["redis-server", "--appendonly", "yes"]

  project-list-pokemons.api:
    build:
      context: .
      dockerfile: project-list-pokemons.Api/Dockerfile
    image: projectlistpokemonsapi
    container_name: project-list-pokemons
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__SQLiteConnection=Data Source=/data/pokemon.db
      - RedisConnection=redis:6379
      - RESET_DATABASE_SQLITE=false
      - SYNC_INTERVAL_HOURS=24
    depends_on:
      - redis
    networks:
      - app_network
    volumes:
      - sqlite_data:/data

volumes:
  sqlite_data:
    driver: local

networks:
  app_network:
    driver: bridge
```

---

## **Configurações de `appsettings`**

### **1. Desenvolvimento**
Arquivo: `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "SQLiteConnection": "Data Source=/data/pokemon_dev.db",
    "RedisConnection": "redis:6379"
  },
  "Jwt": {
    "Key": "SuperSegura123!@#456SuperSegura!@#",
    "Issuer": "project_list_pokemons_api"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/log.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

### **2. Produção**
Arquivo: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "SQLiteConnection": "Data Source=/data/pokemon.db",
    "RedisConnection": "redis:6379"
  },
  "Jwt": {
    "Key": "aqui_sua_chave_secreta_minimo_32_caracteres",
    "Issuer": "project_list_pokemons_api"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/log.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

### **Observação Importante sobre a Chave JWT**  
- **A chave secreta (`Jwt:Key`) deve ser substituída por um valor único e seguro com no mínimo 32 caracteres.**  
  - Essa chave é usada para assinar os tokens JWT e garantir sua validade.
  - Para ambiente de **desenvolvimento**, foi utilizada uma chave genérica para facilitar os testes.  
  - Para **produção**, é obrigatório gerar uma chave segura. Ferramentas como geradores de senha ou bibliotecas de criptografia podem ser utilizadas para criar uma chave adequada.

- **Recomendações:**
  - **Não publique a chave em repositórios públicos ou compartilhe-a sem proteção.**
  - **Utilize variáveis de ambiente para armazenar a chave em produção** em vez de deixá-la diretamente no arquivo `appsettings.json`.
  - Exemplos de geração de chaves:
    - PowerShell:
      ```bash
      [System.Guid]::NewGuid().ToString("N") + [System.Guid]::NewGuid().ToString("N")
      ```
    - Ferramentas online confiáveis como o [LastPass Generator](https://www.lastpass.com/features/password-generator).

---

## **Gerenciamento de Logs**

Este projeto utiliza **Serilog** para geração de logs estruturados. Os logs são configurados para serem gravados em um arquivo local com rotação diária, garantindo que o histórico de execução seja mantido de forma organizada e eficiente.

### **Configuração de Logs**

- Os logs são configurados nos arquivos `appsettings.json` e `appsettings.Development.json`.
- Exemplo de configuração:
  ```json
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/log.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
  ```

### **Onde Encontrar os Logs**
- **Ambiente de Produção:** Os logs são armazenados em `/app/logs/log.txt`.
- **Ambiente de Desenvolvimento:** Os logs seguem o mesmo padrão, mas adaptados ao ambiente de desenvolvimento.

### **Níveis de Log**
- **Information:** Usado para mensagens gerais do sistema.
- **Warning:** Registra situações que podem indicar possíveis problemas futuros.
- **Error:** Para registrar falhas que impactam a funcionalidade.

---

## **Rodando o Projeto**

### **1. Desenvolvimento**
Para iniciar o ambiente de desenvolvimento:
```bash
docker-compose up
```

Para rodar em segundo plano:
```bash
docker-compose up -d
```

### **2. Produção**
Para iniciar o ambiente de produção:
```bash
docker-compose -f docker-compose.production.yml up
```

Para rodar em segundo plano:
```bash
docker-compose -f docker-compose.production.yml up -d
```

---

### **Diferenciação entre os Ambientes**

| Recurso                 | Desenvolvimento                                           | Produção                                                   |
|-------------------------|----------------------------------------------------------|-----------------------------------------------------------|
| **Banco de Dados**      | O banco é recriado sempre que a API é iniciada, garantindo um ambiente limpo para testes. | O banco é criado apenas na primeira execução e mantém os dados para execuções subsequentes. |
| **Sincronização de Dados** | Sincroniza um máximo de 50 Pokémons para testes mais rápidos. | Sincroniza com todos os Pokémons disponíveis na PokeAPI.  |
| **Configurações de Lote** | Lotes menores (10 Pokémons por lote) para facilitar o desenvolvimento. | Lotes maiores (100 Pokémons por lote) para eficiência em produção. |

---

## **Testando a API**

### **1. Acessando a Documentação (Swagger)**
- **Observação Importante:** O Swagger está configurado para funcionar **apenas no ambiente de desenvolvimento**. Certifique-se de que o ambiente esteja configurado como `Development` no `appsettings.Development.json` ou via variável de ambiente:
  ```bash
  ASPNETCORE_ENVIRONMENT=Development
  ```
  
- Acesse o Swagger para explorar e testar os endpoints da API:
  - **URL:** `http://localhost:5000/swagger`
  - O Swagger permite visualizar a documentação dos endpoints e testá-los diretamente.

### **2. Realizando Login**
- Antes de acessar os endpoints protegidos, é necessário realizar o login no endpoint de autenticação:
  - **Endpoint:** `POST /auth/login`
  - **Credenciais:**
    ```json
    {
      "username": "admin",
      "password": "123456789"
    }
    ```

- O endpoint retornará um token JWT que deve ser utilizado nas requisições seguintes.

### **3. Configurando o Token**
- **Postman:**
  - Copie o token JWT retornado pelo login.
  - Adicione o token no cabeçalho de todas as requisições:
    ```http
    Authorization: Bearer <SEU_TOKEN_JWT>
    ```

- **Swagger:**
  - No Swagger, clique no botão **"Authorize"** localizado no canto superior direito.
  - Insira o token no formato:
    ```
    Bearer <SEU_TOKEN_JWT>
    ```
  - Após configurar, os endpoints protegidos estarão disponíveis para teste.

---

## **Rodando os Testes**

1. Navegue até a pasta de testes:
   ```bash
   cd project-list-pokemons/project-list-pokemons.Tests
   ```

2. Execute o comando para rodar os testes:
   ```bash
   dotnet test
   ```

---

## **Estrutura do Projeto**

- **Camada de API:** Contém os endpoints REST para comunicação com o cliente.
- **Camada de Aplicação:** Gerencia as regras de negócio, incluindo validações e interações com a API externa.
- **Camada de Dados:** Configura o Entity Framework Core e gerencia o SQLite.
- **Cache:** Implementação com Redis para otimizar a busca de Pokémons e reduzir requisições redundantes à PokeAPI.
- **Autenticação JWT:** Protege os endpoints com credenciais fixas (apenas para demonstração).

---

## **Desafios e Soluções**

1. **Análise do Desafio e Requisitos da PokeAPI:** 
   - Antes de iniciar o desenvolvimento, foi realizada uma análise detalhada do desafio proposto e da documentação da PokeAPI. Isso permitiu compreender os requisitos do projeto e identificar os principais endpoints necessários para a solução, como busca por Pokémon, espécies, evoluções e sprites. Essa etapa inicial garantiu um planejamento sólido e facilitou a implementação das funcionalidades.

2. **Pokémons Aleatórios:** 
   - Criamos uma lógica de randomização interna para gerar listagens, já que a PokeAPI não possui um endpoint nativo para isso. Essa solução permite retornar 10 Pokémons aleatórios de forma eficiente e consistente.

3. **Requisições à PokeAPI:** 
   - Para evitar bloqueios e reduzir dependências da API externa, sincronizamos os dados com o SQLite a cada 24 horas no ambiente de produção (ou a cada 1 hora no ambiente de desenvolvimento) e utilizamos Redis para armazenamento em cache. Essa abordagem minimiza o número de chamadas à PokeAPI, otimizando o desempenho.

4. **Tratamento de Imagens:** 
   - Implementamos a conversão de sprites fornecidos pela PokeAPI em Base64. As imagens são armazenadas localmente, garantindo acessibilidade mesmo em situações de indisponibilidade da PokeAPI.

5. **Indisponibilidade da PokeAPI:** 
   - Para lidar com situações em que a PokeAPI está offline ou inacessível, o sistema utiliza o SQLite e o Redis como fontes alternativas de dados. Isso assegura a continuidade do serviço para os usuários finais.

6. **Gerenciamento de Logs:**
   - Foi implementado o uso de Serilog para registrar logs estruturados, permitindo rastrear ações, identificar erros e monitorar o funcionamento do sistema em tempo real. Logs são gravados localmente em arquivos e organizados por intervalos diários para facilitar a análise.

---

## **Evolução e Estratégias Futuras**

- Criar integração com serviços de mensageria para sincronização de dados da PokeAPI e melhor gerenciamento de grandes volumes.
- Expandir a sincronização dinâmica com a PokeAPI para fornecer apenas os dados necessários para aplicações específicas.
- Melhorar a normalização e validação dos dados armazenados localmente para maior consistência.

---

## **Apresentação do Projeto**

Assista à apresentação do projeto no link abaixo:

- **[Apresentação em Vídeo](https://drive.google.com/file/d/1529HZJxCzLiN76DOgP_OlaMpjzmD66KB/view?usp=sharing)**

---

## **Links Úteis**

- **Repositório GitHub:** [Backend Challenge: Pokémons](https://github.com/halls510/project-list-pokemons)
- **Documentação da PokeAPI:** [PokeAPI](https://pokeapi.co/docs/v2)

---

**Este é um desafio da [Coodesh](https://coodesh.com/)** 🚀

--- 