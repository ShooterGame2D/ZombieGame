# Defesa Zumbi — Edição Pixel

Protótipo integrado ao projeto ZombieGame. Use a Unity **6000.3.25f1**, versão registrada pelo repositório, com os módulos Windows e WebGL para gerar essas versões.

## Jogar na Unity

1. Abra a pasta ZombieGame pelo Unity Hub.
2. Espere a importação e compilação terminarem.
3. No menu, escolha **Defesa Zumbi > Abrir protótipo**.
4. Pressione **Play**. Escolha um dos oito personagens ou **Ver demonstração**.

A cena de entrada é `Assets/_Project/Scenes/Boot.unity`. A `SampleScene` original continua disponível. O protótipo não inicia automaticamente nas cenas originais do projeto.

Mouse mira; clique esquerdo atira/coleta; Q, E ou Tab alternam frente/costas; Espaço ou botão direito usam a habilidade; Esc pausa; Enter encerra a loja.

## Gerar o jogo para Windows ou navegador

Use **Defesa Zumbi > Gerar Windows** ou **Defesa Zumbi > Gerar Web**. Esses comandos validam as mecânicas e compilam a cena Boot explicitamente, sem alterar a lista de cenas do projeto ou restaurar o balanceamento.

O executável vai para `Builds/Windows/Defesa Zumbi.exe`. O site vai para `Builds/Web`. Os módulos correspondentes precisam estar instalados no Unity Hub. As compilações não são versionadas.

Para jogar a versão web, sirva a pasta `Builds/Web` por HTTP. Se tiver Python instalado, abra um terminal nessa pasta e execute:

```powershell
python -m http.server 8766 --bind 127.0.0.1
```

Acesse http://127.0.0.1:8766/. Abra um terminal na pasta gerada; não abra o HTML diretamente. O template usa nomes de arquivos baseados no conteúdo para evitar misturar versões em cache.

## Editar e validar

- Dados de personagens, armas, ondas, torres e zumbis: `Assets/_Project/Data`.
- Catálogo ativo: `Assets/_Project/Resources/GameCatalog.asset`.
- Código do jogo: `Assets/_Project/Scripts`.
- Arte pixel: `Assets/_Project/Resources/Art/Pixel`.
- **Defesa Zumbi > Validar mecânicas** executa 91 verificações de simulação e áudio.
- **Defesa Zumbi > Restaurar dados iniciais** sobrescreve o balanceamento do protótipo com os valores de `PrototypeBuilder.cs`. Use somente quando quiser restaurá-los.

As configurações URP, Input System, pacotes, nome do produto, versão da Unity e cenas originais foram preservadas. As opções do template WebGL são aplicadas apenas durante a compilação e restauradas ao final. A geração não redefine os dados editados no Inspector.

Os arquivos `.meta` acompanham todos os assets. O repositório usa Git LFS para imagens: mantenha o Git LFS instalado para clonar e publicar os sprites corretamente.

Detalhes de personagens, loja, munição e defesas: [mecânicas](Docs/DefesaZumbi/MECANICAS.md). Origem das artes: [assets pixel](Docs/DefesaZumbi/ASSETS-PIXEL.md).

## Limites do protótipo

Jogo 2D com perspectiva simulada e interface desenhada por código. Os zumbis usam sprites com movimento procedural, sem animação quadro a quadro. Armas e defesas são desenhadas por código; os efeitos sonoros são sintetizados. O balanceamento ainda precisa de testes com jogadores.

O progresso local usa `Application.persistentDataPath` e, na web, PlayerPrefs. O progresso da pasta antiga não é transferido automaticamente, pois o nome do produto e a empresa do repositório foram mantidos.

## Validação desta integração

O código adaptado compilou e passou nas 91 verificações em uma cópia isolada com Unity 6000.3.24f1 e os pacotes do protótipo. As referências dos assets foram verificadas, sem colisões de GUID com o projeto existente. A integração não alterou nenhum arquivo previamente versionado.

A Unity 6000.3.25f1 e a combinação exata dos pacotes do repositório ainda precisam de validação no editor correspondente. A versão do repositório foi preservada; ele não foi aberto com o editor mais antigo.
