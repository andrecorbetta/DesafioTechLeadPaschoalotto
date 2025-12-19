# Desafio Pasch – Listagem de Títulos em Atraso

Este projeto foi desenvolvido como solução para o desafio técnico proposto, contemplando **backend**, **frontend** e **containerização com Docker**, seguindo boas práticas de engenharia de software e mantendo foco em simplicidade, clareza e manutenibilidade.

---

## 🎯 Objetivo do Desafio

Construir uma aplicação capaz de:

- Listar títulos em atraso
- Calcular valor atualizado, multa e juros
- Permitir filtros dinâmicos
- Apresentar os dados de forma clara no frontend
- Disponibilizar tudo de forma containerizada

---

## 🧩 Visão Geral da Solução

A solução é composta por:

- **Backend**: API REST responsável pelas regras de negócio e listagem dos títulos
- **Frontend**: Aplicação Angular para visualização, filtros e interação do usuário
- **Infraestrutura**: Docker + Docker Compose para execução integrada

```
┌──────────────┐     ┌──────────────┐
│   Frontend   │ --> │   Backend    │
│   Angular    │     │   .NET API   │
└──────────────┘     └──────────────┘
        │
        ▼
     Nginx
```

---

## 🛠️ Tecnologias Utilizadas

### Backend
- .NET
- API REST
- DTOs para comunicação
- Separação Controller / Service

### Frontend
- Angular
- Angular Material
- Reactive Forms
- HttpClient
- Nginx para serving do build

### Infraestrutura
- Docker
- Docker Compose

---

## 🧱 Princípios de Engenharia Aplicados

A arquitetura foi pensada para **resolver o problema com qualidade**, evitando complexidade desnecessária.

### KISS & YAGNI
- Apenas o que o desafio exigia foi implementado
- Nenhuma abstração prematura
- Código direto, legível e previsível

### SOLID (aplicação pragmática)
- **SRP**: cada classe tem uma responsabilidade clara
- **DIP**: dependências são injetadas
- **OCP**: filtros e comportamentos podem ser estendidos sem reescrita
- Sem heranças artificiais ou contratos inflados

### Clean Code
- Nomes claros
- Métodos curtos
- Fluxos explícitos (ex.: botão “Buscar”)
- Fácil leitura e manutenção

### Clean Architecture (em escala adequada)
- Separação clara entre:
  - Camada de apresentação
  - Regras de negócio
  - Infraestrutura
- Sem overengineering para o escopo do desafio

> A arquitetura foi desenhada para ser **simples hoje** e **evolutiva amanhã**.

---

## 🚀 Como Executar o Projeto (Docker)

### Pré-requisitos
- Docker
- Docker Compose

---

### 1️⃣ Clonar o repositório

```bash
git clone <url-do-repositorio>
cd Desafio
```

---

### 2️⃣ Build e subida dos containers

```bash
docker compose up -d --build
```

---

### 3️⃣ Acessos

- **Frontend**  
  http://localhost:4200

- **Backend (API)**  
  http://localhost:4200/api/v1/titulos/atrasados

> O frontend consome a API via proxy configurado no Nginx.

- **Backend (API) - Swagger**  
  http://localhost:8080/swagger/index.html
---

### 4️⃣ Parar os containers

```bash
docker compose down
```

---

## 📁 Estrutura Geral

```
Desafio
│
├── backend
│   ├── Controllers
│   ├── Services
│   └── Dockerfile
│
├── frontend
│   ├── src
│   ├── Dockerfile
│   └── nginx.conf
│
└── docker-compose.yml
```

---

## ✅ Considerações Finais

Este projeto prioriza:

- Clareza
- Simplicidade
- Manutenibilidade
- Boas práticas proporcionais ao escopo

A solução evita complexidade desnecessária e demonstra uma abordagem madura de engenharia de software, focada em entregar valor com qualidade.

---

**Autor:**  
André Corbetta de Pauli  
Senior Software Engineer
