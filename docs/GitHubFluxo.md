# Fluxo Git — Hackathon

## 1. Criar sua branch a partir da main atualizada
git checkout main
git pull origin main
git checkout -b feature/backend-produtos

## 2. Trabalhar e commitar (pode fazer várias vezes)
git add .
git commit -m "feat: cria endpoint GET /produtos"

# ... continua trabalhando ...

git add .
git commit -m "feat: cria endpoint POST /produtos"

## 3. Subir sua branch pro repositório remoto
git push origin feature/backend-produtos

## 4. Quando a feature estiver pronta, voltar pra main e atualizar
git checkout main
git pull origin main

## 5. Trazer sua feature pra main
git merge feature/backend-produtos

Se aparecer conflito: abre o(s) arquivo(s) marcado(s), resolve manualmente,
depois:
git add .
git commit -m "merge: resolve conflito com feature/backend-produtos"

## 6. Subir a main atualizada
git push origin main

## 7. (Opcional) Apagar a branch já mergeada
git branch -d feature/backend-produtos
git push origin --delete feature/backend-produtos
---

### Resumo do ciclo completo
git checkout main
git pull origin main
git checkout -b feature/minha-parte

git add .
git commit -m "mensagem clara do que foi feito"
git push origin feature/minha-parte

git checkout main
git pull origin main
git merge feature/minha-parte
git push origin main