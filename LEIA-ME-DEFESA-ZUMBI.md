# Defesa Zumbi — Edição Pixel

Protótipo integrado ao projeto ZombieGame. Use a Unity **6000.3.25f1**, versão registrada pelo repositório, com os módulos Windows e WebGL para gerar essas versões.

## Jogar na Unity

1. Abra a pasta ZombieGame pelo Unity Hub.
2. Espere a importação e compilação terminarem.
3. No menu, escolha **Defesa Zumbi > Abrir protótipo**.
4. Pressione **Play**. A tela inicial oferece Jogar, Tutorial e Configurações.

A cena de entrada é `Assets/_Project/Scenes/Boot.unity`. A `SampleScene` original continua disponível. O protótipo não inicia automaticamente nas cenas originais do projeto.

Mouse mira; M1 atira; M2 coleta moedas; R recarrega antes de acabar o pente; Q, E, Tab ou scroll alternam frente/costas; somente Espaço usa a habilidade; Esc pausa; Enter encerra a loja (exceto no tutorial).

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
- **Defesa Zumbi > Validar mecânicas** executa as cinco suites, totalizando 829 verificações.
- **Defesa Zumbi > Restaurar dados iniciais** sobrescreve o balanceamento do protótipo com os valores de `PrototypeBuilder.cs` e `ExpansionSetup.cs`. Use somente quando quiser restaurá-los.

As configurações URP, Input System, pacotes, nome do produto, versão da Unity e cenas originais foram preservadas. As opções do template WebGL são aplicadas apenas durante a compilação e restauradas ao final. A geração não redefine os dados editados no Inspector.

Os arquivos `.meta` acompanham todos os assets. O repositório usa Git LFS para imagens: mantenha o Git LFS instalado para clonar e publicar os sprites corretamente.

Detalhes de personagens, loja, munição e defesas: [mecânicas](Docs/DefesaZumbi/MECANICAS.md). Origem das artes: [assets pixel](Docs/DefesaZumbi/ASSETS-PIXEL.md).

## Limites do protótipo

Jogo 2D com perspectiva simulada e interface desenhada por código. Os dez tipos de zumbi usam oito quadros de caminhada, com cadência ligada à velocidade real e alinhamento dos pés. A animação continua na câmera escondida, desacelera no gelo e para durante pausa, raízes, isca e ataques estacionários. Armas e defesas usam folhas de sprites; as armas usam gravações, foley e síntese complementar, com sons específicos de disparo e recarga. O balanceamento ainda precisa de testes com jogadores.

O progresso local usa `Application.persistentDataPath` e, na web, PlayerPrefs. O progresso da pasta antiga não é transferido automaticamente, pois o nome do produto e a empresa do repositório foram mantidos.

## Validação desta integração

O código e os assets da atualização 0.4 passaram em 717 verificações na cópia local com Unity 6000.3.24f1, e as versões Windows e Web foram compiladas. O tutorial foi concluído manualmente no navegador, incluindo tiro, cabeça, R, M2, scroll, especial, mini-onda e compra de munição.

A Unity 6000.3.25f1 e a combinação exata dos pacotes do repositório ainda precisam de validação no editor correspondente. A versão do repositório foi preservada; ele não foi aberto com o editor mais antigo.

## Caminhada dos zumbis

As dez folhas já estão ligadas ao combate. O catálogo `Assets/_Project/Resources/ZombieWalkAtlas.asset` referencia os PNGs e define a cadência e os apoios dos pés de cada tipo. As imagens continuam em `Assets/_Project/Art/AnimationDrafts/Walk-v1`, sem duplicação. Não é necessário configurar Animator ou Animation Clips para esta implementação.

O menu **Defesa Zumbi > Validar mecânicas** executa também 127 verificações de animação, além das 92 verificações de mecânicas. Elas cobrem as dez texturas, os 80 apoios, os ciclos, pausa, troca de câmera, gelo, raízes, isca, barreiras e aceleração dos zumbis. A validação e as compilações locais usaram Unity 6000.3.24f1; a versão 6000.3.25f1 deste repositório foi preservada.

## Textos e sangue

Os textos usam a fonte Atkinson Hyperlegible, do Google Fonts, sem sombras. A fonte e a licença SIL OFL acompanham o projeto e a tela de carregamento web, sem depender de internet.

Os acertos geram gotas de sangue no ponto de impacto; as gotas caem e deixam manchas no chão, separadas por câmera. Elas permanecem até o final da onda. Pausa congela os efeitos, reiniciar limpa as manchas, e a aleatoriedade visual não muda a queda de moedas.

A validação inclui 51 verificações de fonte e efeitos, somadas às suites de mecânicas e animações. As versões Windows e Web são compiladas na cópia local com Unity 6000.3.24f1; a versão registrada neste repositório continua preservada.
## Armas, defesas e ataques animados

A pasta `Assets/_Project/Art/Combat-v1` contém 27 folhas transparentes, com seis quadros e recortes de sprites em cada uma: 13 armas, 10 ataques de zumbis, três torres e a barricada com arame farpado. As defesas têm três perspectivas por lane e um segundo estado ativo/danificado. Os prompts da geração integrada estão em `generation-prompts.json`. A galeria `preview.html` mostra animações e permite ouvir os 27 WAVs de referência em `Sounds`.

O catálogo `Resources/CombatSpriteCatalog.asset` liga as artes ao jogo. Não é preciso configurar Animator: os quadros seguem o estado do combate. O menu **Defesa Zumbi > Atualizar arte de combate** reimporta recortes, recalcula os apoios e exporta os sons a partir do código.

O arco prepara a corda antes de disparar e consome uma flecha ao soltar. Trocar de câmera cancela a preparação. Os zumbis animam preparação, golpe e recuperação, inclusive contra barricadas e fora da câmera. A posição das defesas e a ordem de desenho acompanham a profundidade de cada lane. Disparo e recarga têm assinatura sonora própria para cada arma.

Foram acrescentadas 317 verificações de assets, animações, arco, defesas e áudio, somadas às demais suites. O projeto local foi validado com Unity 6000.3.24f1; a versão 6000.3.25f1 registrada neste repositório permanece preservada.

## Atualização 0.4 — controles, especiais e progressão

Novo menu inicial, tutorial de dez etapas com pausas e treino sem dano, configurações persistentes de brilho, sensibilidade, volume geral, efeitos, música e tremor. A sensibilidade usa mira relativa no Windows e curva absoluta na web, compatível com navegadores integrados.

O arco fica de frente e ocupa menos tela; o estilingue mostra a mão puxando. O machado sai da mão e gira até o impacto, e o bumerangue voa mais devagar, com ida e volta. Granada, ursinho, árvore caindo, zumbis emergindo e distraídos receberam seis quadros novos cada. Os oito PNGs transparentes e os prompts estão em `Assets/_Project/Resources/Art/Expansion`; a galeria animada fica em `Assets/_Project/Art/Expansion-v4/preview.html`.

O lenhador abre a seleção com Espaço, clica na coluna do painel inferior e confirma com outro Espaço, eliminando os zumbis dessa coluna e câmera. ESC ou trocar de câmera cancela sem custo. O cowboy recebe bônus de dano, filtro alaranjado e câmera lenta por cinco segundos reais. O corredor foi ampliado para manter os obstáculos nas laterais.

Chefões aparecem a cada cinco ondas, alternando quatro variantes, com retrato e barra superior. Chefes anteriores retornam como elites a partir da onda oito. As ondas ficam intensas mais cedo, torres foram enfraquecidas e encarecidas, munição ficou mais cara, a loja indica nível máximo e permite substituir torres preservando a barricada. Há três segundos de contagem no fim da onda, avisos fortes e tremor contextual.

Os 30 efeitos em `Resources/Audio/Recorded` têm créditos e licença CC0 em `CREDITS.txt`. Oito temas originais por cenário estão em `Art/Expansion-v4/Music`. Novas imagens foram criadas com o gerador integrado; os prompts acompanham os PNGs.


## Atualização 0.5 — novos mapas e personagens compráveis

Os 16 fundos de frente/costas foram redesenhados para a área de surgimento existente. A arte fica em `Assets/_Project/Resources/Art/Maps-v5`, junto aos prompts do gerador integrado; a galeria em `Assets/_Project/Art/Maps-v5/preview.html`. A camada opaca que cobria o mapa e as marcações do grid foram removidas. Coordenadas de surgimento e projeção não mudaram.

O Recruta é gratuito. Criança custa 60 gemas, Indígena 80, Engenheiro 100, Cowboy 140, Canguru 180, Lenhador 220 e Ninja 260. Os desbloqueios são permanentes, a compra seleciona o personagem e não pode cobrar novamente por um personagem comprado. O menu mostra o saldo e quanto falta. Saves da prévia com todos liberados (perfil v2) mantêm gemas e recorde e começam a nova dinâmica com o Recruta; compras do perfil v3 persistem.

O especial do lenhador agora exige dois Espaços. O primeiro mostra esquerda/centro/direita na interface; um clique na coluna do painel inferior seleciona o alvo e o segundo confirma a árvore. Só a confirmação gasta o especial. ESC e troca de câmera cancelam; a onda segue normalmente durante a seleção. Não há grid desenhado no chão nem durante a escolha.

Use **Defesa Zumbi > Aplicar mapas e desbloqueios 0.5** apenas para restaurar preços e importação desta atualização. Os valores atuais já acompanham o projeto. As 829 verificações automatizadas e as compilações locais usam Unity 6000.3.24f1; a versão 6000.3.25f1 e os pacotes deste repositório foram preservados.


## Atualização 0.5.1 — prévia e seleção por clique

Clicar em qualquer cartão mostra retrato, vida, cenário, arma, especial e recarga, sem comprar nem trocar o personagem em uso. A compra e a troca ficam nos botões explícitos do painel de detalhes. Os cartões aparecem em ordem crescente de preço: Recruta, Criança, Indígena, Engenheiro, Cowboy, Canguru, Lenhador e Ninja.

O primeiro Espaço do lenhador abre três botões na parte inferior da tela. Clique para selecionar esquerda, centro ou direita; passar o mouse não altera a escolha. Outro Espaço confirma a coluna selecionada. Sem clique, o especial não dispara. O clique não atira a arma. ESC ou trocar de câmera cancela sem consumir a habilidade.
