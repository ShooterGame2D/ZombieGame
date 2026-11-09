# Z-Hold — Edição Pixel

Versão 0.5 do protótipo, com visual inteiramente em pixel art, feita na Unity 6000.3.24f1. O jogador protege sua posição contra zumbis que vêm da frente e das costas. Os dois lados continuam em movimento o tempo todo.

## Jogar

Abra **JOGAR.cmd** nesta pasta ou `Builds/Windows/Z-Hold.exe`. A versão de navegador fica em `Builds/Web` e precisa ser servida por HTTP. O endereço usado nesta sessão é http://127.0.0.1:8765/; recarregue a página para receber a nova versão.

A tela inicial tem **Jogar**, **Tutorial** e **Configurações**. Em Jogar, escolha um personagem e clique em **Jogar: nome do personagem**. O tutorial ensina em dez etapas, com explicações pausadas, uma mini-onda sem perda de vida e uma compra real na loja de treino. O Recruta começa desbloqueado. Os demais personagens são compras permanentes no menu, pagas com gemas obtidas ao encerrar partidas; gemas, desbloqueios e recorde são salvos. As demonstrações não gravam recompensas.

## Controles

| Ação | Controle |
| --- | --- |
| Mirar | Mouse |
| Disparar | Clique esquerdo; segure para disparar na cadência da arma |
| Coletar moeda | Mire nela e clique com o botão direito (M2) |
| Recarregar antes de esvaziar | R |
| Alternar frente/costas | Q, E, Tab ou rodinha do mouse |
| Habilidade especial | Somente Espaço |
| Próxima onda na loja | Enter |
| Pausar | Esc |

O radar mostra frente na metade superior e costas na inferior, com sua posição no centro. A metade destacada é a câmera atual. Pontos verdes representam inimigos visíveis, amarelos os da outra câmera e vermelhos os muito próximos. Um aviso vermelho informa ataques recebidos fora da câmera; o aviso âmbar indica aproximação perigosa. Os zumbis, torres, moedas e projéteis da câmera escondida continuam funcionando.

## Personagens e habilidades

| Personagem | Arma inicial | Vida | Habilidade | Recarga | Cenário |
| --- | --- | ---: | --- | ---: | --- |
| Recruta | Pistola gasta | 110 | Granada arremessada: 16 de dano ao cair | 22 s | Posto militar |
| Indígena | Arco simples | 95 | Raízes: prende o lado atual por 5 s | 24 s | Amazônia |
| Engenheiro | Parafusadeira de pregos | 105 | Torre automática no lado atual por 10 s | 30 s | Canteiro de obras |
| Cowboy | Revólver antigo | 100 | Olho de águia: dano 2,5×, visão âmbar e mundo a 32% da velocidade por 5 s | 26 s | Faroeste |
| Canguru | Bumerangue | 125 | Salto sísmico: dano e empurrão no lado atual | 23 s | Outback |
| Criança | Estilingue em Y | 80 | Isca: distrai o lado atual durante 6 s | 22 s | Pátio da escola |
| Lenhador | Machado de arremesso | 135 | Árvore caindo: Espaço abre a seleção, um clique na coluna do painel inferior seleciona o alvo e outro Espaço confirma | 26 s | Serraria na floresta |
| Ninja | Shuriken de treino | 90 | Fumaça: invulnerabilidade por 5 s e rajada no lado atual | 30 s | Templo ao luar |

As armas iniciais têm potência inicial semelhante; o machado causa 2,1 de dano por arremesso (intervalo mínimo 0,8 s) e o bumerangue 3,8 (intervalo 1,5 s), compensando o voo mais lento. A identidade principal vem da habilidade, vida, visual e cenário. O machado é arremessado para permitir o mesmo combate de mira fixa. Arco, estilingue e armas arremessáveis usam lotes de projéteis equivalentes ao pente do protótipo.

## Munição e arsenal

Não existe regeneração de munição. R inicia uma recarga parcial, preservando as balas já no pente e consumindo apenas as que faltam na reserva. O pente vazio continua iniciando a recarga automaticamente, desde que exista reserva. O jogo continua durante a recarga, inclusive após trocar de câmera. A transferência da reserva para o pente acontece quando o tempo termina; reserva insuficiente carrega apenas a quantidade disponível. Coletar moedas não gasta tiros e continua possível durante a recarga.

A loja tem três abas: **Armas**, **Melhorias e munição** e **Torres e barricadas**. Depois de eliminar os inimigos, um contador de 3 segundos encerra a onda; moedas restantes são recolhidas automaticamente. O intervalo de compras dura 35 segundos para permitir compras nos dois lados, podendo ser encerrado por Enter.

Todos acessam o mesmo arsenal e os mesmos preços:

| Arma | Preço | Dano | Pente | Recarga | Diferencial |
| --- | ---: | ---: | ---: | ---: | --- |
| Submetralhadora | 55 | 1,5 | 24 | 2,1 s | Cadência 7/s |
| Escopeta | 70 | 4 | 6 | 3 s | Dano em área curta |
| Carabina | 90 | 2,5 | 16 | 2,2 s | Boa cadência e precisão |
| Besta de precisão | 100 | 4,5 | 5 | 2,4 s | Cabeça 3× |
| Lança-granadas | 145 | 7 | 4 | 3,4 s | Explosão na mira |

A compra de arma inclui dois pentes de reserva. A munição carregada da arma anterior volta para a reserva, e a nova arma inicia uma recarga. Armas já compradas podem ser reequipadas gratuitamente na loja. As melhorias da partida acompanham o jogador ao trocar de arma.

Um pacote de 36 unidades de munição custa 24 moedas, mais 4 a cada cinco ondas e serve para a arma equipada, inclusive flechas e objetos arremessáveis. Pente maior adiciona duas unidades por nível. Recarga rápida reduz o tempo em 16% por nível, com mínimo de 0,65 s. Há também melhorias de dano, cadência e cura de 30% da vida.

## Torres e barricadas

Cada lado tem três posições independentes de torre. A posição da barricada fica mais longe do jogador, no mesmo alinhamento da torre: **horda → barricada → torre → jogador**.

Uma barricada só pode ser comprada se já houver uma torre atrás dela. Ela ocupa um espaço separado, recebe os ataques primeiro e pode ser consertada ou reforçada. Para comprar defesas do outro lado, troque a câmera no painel de torres ou use Q/E. Atiradora custa 90, Gelo 120, Explosiva 155 e Barricada 45 moedas. A atiradora causa 1,05 a cada 1,65 s; a explosiva 2,6 a cada 4 s; gelo reduz a velocidade em 35%. Melhorias acrescentam 30% do dano base por nível. É possível substituir uma torre por outro modelo pagando o preço completo: o nível reinicia e a barricada permanece. Torres/barricadas indicam nível máximo no nível 4; melhorias de arma, no 5.

## Zumbis e economia

| Tipo | Primeira onda | Comportamento adicional | Valor quando deixa moedas |
| --- | ---: | --- | ---: |
| Errante | 1 | Lento, frágil | 3 |
| Andador | 1 | Mais resistente | 4 |
| Corredor | 2 | Avanço rápido | 5 |
| Cuspidor | 3 | Para e cospe; o projétil pode ser destruído | 7 |
| Blindado | 3 | Reduz dano no corpo; cabeça exposta | 8 |
| Instável | 4 | Explode ao morrer, atingindo zumbis próximos do mesmo lado | 7 |
| Brutamontes | 5 | Mais vida e dano | 12 |
| Médico | 5 | Cura aliados próximos do mesmo lado | 9 |
| Sombra | 6 | Acelera quando está fora da câmera | 10 |
| Chefes | A cada 5 ondas | Comandante invoca; Couraçado resiste a tiros no corpo; Pestilento cospe em três faixas; Colosso avança em arrancadas | 40 + 10 × categoria |

A queda de moedas é aleatória: 52% nos primeiros quatro tipos, 62% nos demais e 100% no comandante. Cada moeda mantém o valor de seu tipo e expira após quatro segundos, inclusive fora da câmera. O radar também marca moedas do lado escondido com pontos pequenos.

Para que a compra de munição não dependa apenas da sorte, vencer uma onda concede um bônus de `24 + 4 × número da onda` moedas. O valor aparece na loja. Esses números são balanceamento inicial ajustável.

## Arte e som

Há dez sprites de zumbis, oito retratos de personagens e dezesseis vistas de cenário (frente e costas para cada personagem). As novas artes foram geradas por IA para este projeto e são carregadas de `Assets/_Project/Resources/Art`. Os assets atuais estão em `Assets/_Project/Resources/Art/Pixel`; os prompts estão em `docs/ASSETS-PIXEL.md`. A edição anterior foi preservada em `Assets/_Project/ArtArchive`.

A edição pixel usa contornos escuros, paleta reduzida, filtro Point sem suavização e painéis em blocos. Os textos usam Atkinson Hyperlegible, do Google Fonts, sem sombras. A fonte original e sua licença SIL Open Font License estão incluídas em `Assets/_Project/Resources/Fonts`, funcionando também offline.

A arte de combate fica em `Assets/_Project/Art/Combat-v1`: 13 folhas de armas, 10 folhas de ataques de zumbis, três torres e uma barricada, com seis quadros por folha (162 sprites). Os PNGs mantêm transparência e os recortes ficam disponíveis como sprites na Unity. O catálogo `Resources/CombatSpriteCatalog.asset` liga as imagens ao jogo. A geração usou a ferramenta integrada de imagens; os prompts estão em `generation-prompts.json`.

As torres e barricadas têm vistas específicas para as lanes esquerda, central e direita. As barricadas possuem arame farpado, madeira, reforços metálicos e uma versão danificada abaixo de 50% de vida. A posição e a ordem de desenho usam a mesma profundidade do combate; as três lanes ficam invisíveis, sem marcas sobre a imagem. Os ataques dos zumbis têm preparação, golpe e recuperação, incluindo a animação do cuspidor ao lançar o projétil. Pausa e raízes congelam os quadros.

O arco puxa a corda antes de soltar a flecha. Um clique prepara e dispara uma flecha; segurar repete na cadência da arma. A munição só é consumida ao soltar, e trocar a câmera cancela a preparação sem gastar flechas. Cada arma tem sons próprios de disparo e recarga. Os 27 arquivos WAV de referência estão em `Combat-v1/Sounds`; a galeria `Combat-v1/preview.html` permite comparar sprites, perspectivas e sons.

Acertos nos zumbis lançam partículas de sangue a partir do ponto atingido. As gotas caem e deixam manchas fixas no chão de cada câmera, apagadas ao finalizar a onda ou reiniciar a partida. Acertos na cabeça e mortes produzem rajadas mais fortes; pausar também congela as partículas. Os efeitos têm limites de quantidade e não interferem na aleatoriedade das moedas.

Gravações e foley complementam os sons sintetizados em camadas: disparos de armas de fogo, corda de arco, elástico, motor de pregador, arremessos, recarga, metal, madeira, moedas, gemidos e habilidades. Efeitos de inimigos fora da câmera têm volume reduzido; os avisos visuais identificam a direção sem depender do áudio. As configurações oferecem volume geral, efeitos, música, brilho, sensibilidade e intensidade de tremor, salvos localmente.

## Editar na Unity

1. Adicione esta pasta no Unity Hub como projeto existente.
2. Abra com Unity **6000.3.25f1**.
3. Abra `Assets/_Project/Scenes/Boot.unity` e pressione **Play**.

Os dados editáveis ficam em `Assets/_Project/Data`. O catálogo ativo fica em `Assets/_Project/Resources/GameCatalog.asset`. Não é necessário conectar objetos manualmente no Inspector.

O menu **Z-Hold > Preparar projeto** restaura os valores iniciais da versão 0.5. As compilações validam e usam os dados atuais, sem restaurar o balanceamento.

- `Scripts/Run/RunManager.cs`: simulação dos dois lados, munição, compras e habilidades.
- `Scripts/UI/GameApplication.cs`: entrada, radar, alertas e menus.
- `Scripts/View/GameArtV2.cs`: sprites, cenários, armas e retratos.
- `Scripts/View/DepthProjector.cs`: distância lógica para posição e escala.
- `Scripts/Audio/PrototypeAudio.cs`: síntese e reprodução dos efeitos.
- `Editor/PrototypeValidation.cs`: testes das mecânicas e integridade dos sons.

## Progresso e limites

O save do Windows fica em `%USERPROFILE%/AppData/LocalLow/Quintal Studio/Z-Hold/profile.json`. A versão web usa o armazenamento local da Unity para o endereço usado. Os dois saves são independentes. A migração do protótipo antigo preserva gemas e recorde e adapta a seleção para o novo elenco.

Esta versão permanece um protótipo 2D com perspectiva simulada e renderizador padrão da Unity. Os dez tipos de zumbi têm caminhada com oito quadros, sincronizada à velocidade real e com alinhamento dos pés. A sequência continua na câmera escondida, desacelera no gelo e para durante pausa, raízes, isca e ataques estacionários. As folhas ficam em `Assets/_Project/Art/AnimationDrafts/Walk-v1`; o catálogo de execução está em `Assets/_Project/Resources/ZombieWalkAtlas.asset`. Armas e defesas usam folhas de sprites; o chão livre e os efeitos complementares usam desenho por código. A interface usa desenho imediato; os tiros são resolvidos por áreas de cabeça/corpo e machados e bumerangues têm tempo de voo; arco e estilingue têm preparação antes de disparar. Só existem os lados frente e costas. Preços, probabilidades e dificuldade ainda precisam de teste de balanceamento com jogadores.

## Validação anterior (0.4)

As compilações Windows e WebGL passaram em 717 verificações automatizadas: 92 de mecânicas, 127 de caminhada, 51 de texto/sangue, 317 de animações de combate e 130 da atualização 0.4. O teste manual no navegador conferiu menu, configurações, tiro, cabeça, recarga antecipada, M2, scroll, especial e contador entre ondas.

Capturas da edição pixel estão na pasta Preview, com nomes iniciados por pixel-. As capturas v2- registram a edição anterior.


### Correção do carregamento web

O carregamento agora identifica os arquivos da compilação e evita reutilizar dados antigos do cache da Unity. Isso corrige a falha de inicialização causada pela mistura de versões, preservando o progresso salvo. As próximas compilações usam nomes baseados no conteúdo dos arquivos. Para iniciar o servidor e abrir o navegador, use JOGAR-WEB.cmd; se ele já estiver ativo, o atalho reutiliza o servidor.




## Atualização 0.4

O terreno usa um corredor amplo sem obstáculos nas três colunas. Todos os zumbis surgem da terra antes de avançar; a isca de ursinho troca sua animação para distração. Novos sprites de arco, estilingue, machado, granada, árvore, ursinho, emergência e distração estão em `Assets/_Project/Resources/Art/Expansion`, com seis quadros cada e prompts de geração integrados. A galeria está em `Assets/_Project/Art/Expansion-v4/preview.html`.

Chefes principais aparecem nas ondas 5, 10, 15, 20 e assim por diante, alternando quatro variantes. A partir da onda 8, chefes de categorias já enfrentadas retornam como elites com 60% da vida. O chefe principal tem prioridade na barra superior, que informa também sua câmera.

No Windows, a sensibilidade controla uma mira relativa com captura do mouse durante o combate. Na web, usa uma curva de resposta absoluta que mantém centro e bordas acessíveis e evita depender de pointer lock em navegadores integrados. ESC pausa. O HUD fica estável durante os tremores do cenário. A câmera lenta do cowboy afeta as duas frentes, ataques e projéteis, mas dura cinco segundos reais.

Os 30 sons gravados/editados estão em `Assets/_Project/Resources/Audio/Recorded`, com créditos CC0 e manifestos. As famílias de armas de fogo, arco e besta usam gravações; arremessos e mecanismos usam foley. A síntese original complementa explosões, impactos e avisos. Oito trilhas originais por cenário são geradas por `MapMusic.cs` e exportadas em `Art/Expansion-v4/Music`.


## Atualização 0.5 — mapas, gemas e seleção da árvore

Os oito mapas têm novas vistas de frente e costas em `Assets/_Project/Resources/Art/Maps-v5/Front.png` e `Back.png`. As folhas têm duas colunas e quatro linhas, na ordem dos personagens. O jogo usa a parte inferior de cada célula para alinhar o chão com a linha de surgimento fixa em y=400. A projeção, os pontos de surgimento e as posições das três colunas permanecem os mesmos. A camada opaca de chão e as marcações foram removidas. A galeria fica em `Assets/_Project/Art/Maps-v5/preview.html`; os prompts acompanham os PNGs.

| Personagem | Gemas para desbloquear |
| --- | ---: |
| Recruta | Grátis |
| Criança | 60 |
| Indígena | 80 |
| Engenheiro | 100 |
| Cowboy | 140 |
| Canguru | 180 |
| Lenhador | 220 |
| Ninja | 260 |

Comprar seleciona o personagem e desconta o preço uma única vez. Depois disso, trocar entre personagens comprados é gratuito. O menu informa quanto falta quando não há gemas suficientes. As recompensas continuam sendo calculadas ao fim da partida: `máximo(1, 10 × ondas concluídas + eliminações / 5 + 50 se houver novo recorde)`.

Saves da versão 0.4 (perfil v2) preservam gemas e recorde, mas deixam de tratar o elenco liberado para teste como compras. O Recruta fica disponível e a seleção volta para ele. Saves novos (perfil v3) preservam os desbloqueios permanentemente, inclusive após reiniciar. A migração do elenco antigo de quatro personagens mantém os desbloqueios registrados.

O primeiro Espaço do lenhador apenas entra na seleção, sem gastar a habilidade. Clique em esquerda, centro ou direita no painel inferior e aperte Espaço novamente: só então a árvore cai e começa a recarga. ESC ou trocar de câmera cancela sem custo. A indicação é uma faixa na interface, sem desenhar grid no chão. A onda continua durante a escolha; os demais personagens continuam usando um único Espaço.


A validação 0.5 acrescenta 112 verificações de mapas, projeção, compras, migração de saves e seleção da árvore, totalizando 829. A validação e as compilações locais usam Unity 6000.3.24f1. Os testes simulam compras em perfis isolados, sem salvar sobre o progresso do jogador.


## Atualização 0.5.1 — prévia e seleção por clique

Clicar em qualquer cartão mostra retrato, vida, cenário, arma, especial e recarga, sem comprar nem trocar o personagem em uso. A compra e a troca ficam nos botões explícitos do painel de detalhes. Os cartões aparecem em ordem crescente de preço: Recruta, Criança, Indígena, Engenheiro, Cowboy, Canguru, Lenhador e Ninja.

O primeiro Espaço do lenhador abre três botões na parte inferior da tela. Clique para selecionar esquerda, centro ou direita; passar o mouse não altera a escolha. Outro Espaço confirma a coluna selecionada. Sem clique, o especial não dispara. O clique não atira a arma. ESC ou trocar de câmera cancela sem consumir a habilidade.


## Z-Hold — abertura 0.6

O jogo agora se chama **Z-Hold**. Ao terminar o carregamento, reproduz a cutscene original de aproximadamente cinco segundos antes da tela inicial. O vídeo está em `Assets/StreamingAssets/Cinematics/Z-Hold-Intro.mp4` e acompanha as versões Windows e Web. A opção **Pular introdução** ou ESC avança para o menu. O vídeo começa automaticamente. No navegador, se o áudio automático for bloqueado, a reprodução continua sem som, com o botão opcional **Ativar som**; não é necessário clicar para assistir. Uma falha de vídeo não bloqueia o acesso ao jogo.

A música do menu e os controles de jogo só são ativados após a introdução. O vídeo mantém a proporção original, com faixas pretas quando necessário, e respeita o volume geral. A cutscene reaparece ao abrir o jogo, sem se repetir ao retornar ao menu durante a mesma sessão.

Gemas, personagens comprados e recorde de versões anteriores são importados automaticamente. As únicas identificações antigas no código ativo são compatibilidade de save e os respectivos testes. O executável passa a ser `Z-Hold.exe`, os menus do editor usam **Z-Hold** e a documentação do repositório fica em `Docs/ZHold`.
