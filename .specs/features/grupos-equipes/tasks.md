# Grupos/Equipes — Tasks

**Status:** CONCLUÍDO  
**Data:** 2026-07-28

## Backend ✅

- [x] Grupo entity (Domain)
- [x] IGrupoRepository interface
- [x] GrupoConfiguration (Fluent API)
- [x] Update UsuarioPerfil: GrupoId, Grupo nav, DefinirGrupo method
- [x] Update UsuarioPerfilConfiguration: FK relationship
- [x] Migration AddGrupo
- [x] GrupoRepository implementation
- [x] DatabaseSeeder: 6 grupos + Fábio→Reembolso
- [x] Grupos CRUD: Commands, Handlers, Validators, Queries, DTOs
- [x] GruposController (CRUD, Admin only)
- [x] Update ICurrentUserService: GrupoId
- [x] Update JwtTokenService: grupo_id claim
- [x] Update CurrentUserService: read grupo_id from JWT
- [x] Update ChamadoRepository.ListarAsync: group-based RBAC
- [x] Update IChamadoRepository: new filter params
- [x] Update ListarChamadosQuery/Handler: pass grupoId
- [x] Update Usuarios CRUD: Criar/Atualizar com GrupoId
- [x] Update UsuarioPerfilResponse: GrupoId, GrupoNome
- [x] DI registration in Program.cs
- [x] 218 testes passando

## Frontend ✅

- [x] GrupoResponse type + UsuarioPerfilResponse update (types/api.ts)
- [x] Grupos API functions (features/admin/api.ts)
- [x] useGrupos, useCriarGrupo, useAtualizarGrupo hooks
- [x] GruposPage (Admin-only, table + CRUD)
- [x] GrupoFormDialog (create/edit, react-hook-form)
- [x] UsuarioFormDialog: Grupo dropdown
- [x] UsuariosPage: Grupo column
- [x] AppLayout: Grupos sidebar link (Admin)
- [x] App.tsx: /admin/grupos route
- [x] 0 erros TypeScript
