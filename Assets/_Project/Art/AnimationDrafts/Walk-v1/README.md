# Estudos de caminhada — zumbis pixel v1

10 folhas PNG RGBA, com 8 poses por personagem: 80 quadros usados na caminhada durante o combate. As artes foram geradas a partir do atlas original com a ferramenta integrada image_gen. Os PNGs foram preservados sem edição posterior. Prompts completos em `generation-prompts.json`.

## Visualizar

Abra `preview.html` no navegador. Funciona também por duplo clique, sem instalação. Escolha o zumbi, pause, avance um quadro ou altere a velocidade. O modo Galeria compara todos os tipos. O fundo claro ajuda a conferir as bordas.

## Usar na Unity

As imagens já têm arquivos `.meta` de Sprite (2D and UI), Multiple, Point, sem mipmaps, sem compressão e com oito recortes nomeados. Ao importar, expanda cada PNG no painel Project para ver os quadros. Mantenha os `.meta` ao copiar.

Importação conferida na Unity 6000.3.24f1: 10 folhas, 80 sprites, dimensões, ordem, filtro e pivôs corretos. O menu **Defesa Zumbi > Validar folhas de caminhada** repete essa checagem no editor que você estiver usando.

- Folha: 1536 × 1024 px.
- Grade: 4 colunas × 2 linhas; célula de 384 × 512 px.
- Ordem: esquerda para direita na linha superior (01–04), depois linha inferior (05–08).
- Pivô comum: X 0,5 / Y 0,09375, próximo à base dos pés.
- Escala: 100 Pixels Per Unit; ajuste a escala do objeto inteiro, não cada quadro.
- Comece com 8–10 quadros/s; corredor 14; tipos pesados 7. São pontos de partida ajustáveis.
- Se usar SpriteRenderer, selecione os quadros 01–08 em ordem e crie um Animation Clip com Loop Time.

As folhas já estão integradas ao combate por `GUI.DrawTextureWithTexCoords`. O catálogo `Assets/_Project/Resources/ZombieWalkAtlas.asset` referencia as dez texturas e permite ajustar a cadência de cada tipo. `ZombieWalkAnimation.cs` seleciona o quadro pela distância realmente percorrida, mantém a escala e alinha os pés ao chão. A câmera escondida continua atualizando; pausa, raízes, isca, gelo e bloqueio por defesas respeitam a simulação. A imagem original serve como fallback caso falte uma folha.

Não é necessário criar Animator ou Animation Clips para jogar esta versão. `frames.json` mantém os retângulos originais para reutilização. O menu **Defesa Zumbi > Validar mecânicas** também executa 127 verificações de animação, incluindo as dez texturas, os 80 apoios dos pés, a troca de câmera e as condições que param ou alteram a cadência.

## Direção do movimento e acabamento

Todos caminham de frente para o jogador, adequado às duas câmeras do jogo. A sequência busca alternar apoio, compressão e passagem das pernas, com balanço contrário dos braços. Não contém ataque, dano ou morte.

São estudos gerados por IA: existem pequenas variações de desenho, apoio dos pés e bordas semitransparentes entre quadros. Antes de fechar a arte de produção, revise o ciclo no fundo claro/escuro e retoque as transições e halos que incomodarem. A prévia reproduz os quadros originais, sem interpolação ou limpeza de pixels.
