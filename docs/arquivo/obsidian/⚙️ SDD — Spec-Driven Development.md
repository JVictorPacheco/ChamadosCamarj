# ⚙️ SDD — Spec-Driven Development

## O que é?

Metodologia onde **primeiro escrevemos o SPEC**, depois implementamos cada parte seguindo ele. A ideia é:

1. **Spec first** — Tudo que vamos construir é descrito antes
2. **Validação** — O spec é revisado e aprovado por você
3. **Implementação** — Cada seção do spec vira código
4. **Verificação** — O código é comparado com o spec

## Como estamos aplicando

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  SPEC    │ →  │  DOMAIN  │ →  │  CQRS    │ →  │  FRONT   │
│ (agora)  │    │ (Fase 1) │    │ (Fase 2) │    │ (Fase 3) │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
```

## Vantagens

- ✅ Você sabe **exatamente** o que vai ser entregue
- ✅ Menos retrabalho — decisões são tomadas antes
- ✅ Documentação viva — o spec É a documentação
- ✅ Pair programming guiado pelo spec

---

> *"Primeiro fazemos funcionar, depois fazemos direito, depois fazemos rápido."*
