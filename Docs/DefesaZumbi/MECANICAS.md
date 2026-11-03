# Mecânicas da edição pixel

## Controles

| Ação | Controle |
| --- | --- |
| Mirar | Mouse |
| Disparar ou coletar | Clique esquerdo; segure para disparar na cadência da arma |
| Alternar frente/costas | Q, E, Tab ou botão de troca na tela |
| Habilidade especial | Espaço ou botão direito |
| Próxima onda na loja | Enter |
| Pausar | Esc |

O radar mostra frente na metade superior e costas na inferior, com sua posição no centro. A metade destacada é a câmera atual. Pontos verdes representam inimigos visíveis, amarelos os da outra câmera e vermelhos os muito próximos. Um aviso vermelho informa ataques recebidos fora da câmera; o aviso âmbar indica aproximação perigosa. Os zumbis, torres, moedas e projéteis da câmera escondida continuam funcionando.

## Personagens e habilidades

| Personagem | Arma inicial | Vida | Habilidade | Recarga | Cenário |
| --- | --- | ---: | --- | ---: | --- |
| Recruta | Pistola gasta | 110 | Granada: 12 de dano em área na mira | 22 s | Posto militar |
| Indígena | Arco simples | 95 | Raízes: prende o lado atual por 5 s | 24 s | Amazônia |
| Engenheiro | Parafusadeira de pregos | 105 | Torre automática no lado atual por 10 s | 30 s | Canteiro de obras |
| Cowboy | Revólver antigo | 100 | Olho de águia: dano 2,5× durante 6 s | 26 s | Faroeste |
| Canguru | Bumerangue | 125 | Salto sísmico: dano e empurrão no lado atual | 23 s | Outback |
| Criança | Estilingue em Y | 80 | Isca: distrai o lado atual durante 6 s | 22 s | Pátio da escola |
| Lenhador | Machado de arremesso | 135 | Corte devastador: 18 de dano em área na mira | 26 s | Serraria na floresta |
| Ninja | Shuriken de treino | 90 | Fumaça: invulnerabilidade por 5 s e rajada no lado atual | 30 s | Templo ao luar |

As armas iniciais ficam próximas em potência: dano 0,95–1,10, cadência 2,4–2,9/s e recarga 2,3–2,6 s. A identidade principal vem da habilidade, vida, visual e cenário. O machado é arremessado para permitir o mesmo combate de mira fixa. Arco, estilingue e armas arremessáveis usam lotes de projéteis equivalentes ao pente do protótipo.

## Munição e arsenal

Não existe regeneração de munição. O pente só inicia recarga ao ficar vazio, desde que exista reserva. O jogo continua durante a recarga, inclusive após trocar de câmera. A transferência da reserva para o pente acontece quando o tempo termina; reserva insuficiente carrega apenas a quantidade disponível. Coletar moedas não gasta tiros e continua possível durante a recarga.

A loja tem três abas: **Armas**, **Melhorias e munição** e **Torres e barricadas**. O intervalo dura 45 segundos para permitir compras nos dois lados, podendo ser encerrado por Enter.

Todos acessam o mesmo arsenal e os mesmos preços:

| Arma | Preço | Dano | Pente | Recarga | Diferencial |
| --- | ---: | ---: | ---: | ---: | --- |
| Submetralhadora | 55 | 1,5 | 24 | 2,1 s | Cadência 7/s |
| Escopeta | 70 | 4 | 6 | 3 s | Dano em área curta |
| Carabina | 90 | 2,5 | 16 | 2,2 s | Boa cadência e precisão |
| Besta de precisão | 100 | 4,5 | 5 | 2,4 s | Cabeça 3× |
| Lança-granadas | 145 | 7 | 4 | 3,4 s | Explosão na mira |

A compra de arma inclui dois pentes de reserva. A munição carregada da arma anterior volta para a reserva, e a nova arma inicia uma recarga. Armas já compradas podem ser reequipadas gratuitamente na loja. As melhorias da partida acompanham o jogador ao trocar de arma.

Um pacote de 36 unidades de munição custa 12 moedas e serve para a arma equipada, inclusive flechas e objetos arremessáveis. Pente maior adiciona duas unidades por nível. Recarga rápida reduz o tempo em 16% por nível, com mínimo de 0,65 s. Há também melhorias de dano, cadência e cura de 30% da vida.

## Torres e barricadas

Cada lado tem três posições independentes de torre. A posição da barricada fica mais longe do jogador, no mesmo alinhamento da torre: **horda → barricada → torre → jogador**.

Uma barricada só pode ser comprada se já houver uma torre atrás dela. Ela ocupa um espaço separado, recebe os ataques primeiro e pode ser consertada ou reforçada. Para comprar defesas do outro lado, troque a câmera no painel de torres ou use Q/E. Atiradora custa 45, Gelo 60, Explosiva 80 e Barricada 25 moedas.

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
| Comandante | A cada 10 ondas | Mais vida e invoca errantes | 50 |

A queda de moedas é aleatória: 52% nos primeiros quatro tipos, 62% nos demais e 100% no comandante. Cada moeda mantém o valor de seu tipo e expira após quatro segundos, inclusive fora da câmera. O radar também marca moedas do lado escondido com pontos pequenos.

Para que a compra de munição não dependa apenas da sorte, vencer uma onda concede um bônus de `12 + 3 × número da onda` moedas. O valor aparece na loja. Esses números são balanceamento inicial ajustável.

## Arte e som

Há dez sprites de zumbis, oito retratos de personagens e dezesseis vistas de cenário (frente e costas para cada personagem). As novas artes foram geradas por IA para este projeto e são carregadas de `Assets/_Project/Resources/Art`. Os assets atuais estão em `Assets/_Project/Resources/Art/Pixel`; os prompts estão em `ASSETS-PIXEL.md`.

A edição pixel usa contornos escuros, paleta reduzida, filtro Point sem suavização, fonte bitmap própria com acentos e painéis em blocos. Armas, torres, barricadas, moedas, mira e efeitos também são desenhados sobre uma grade de pixels. Os sprites de zumbis seguem a referência enviada; os cenários mantêm vistas diferentes de frente e costas.

Os sons são sintetizados em camadas: disparos de armas de fogo, corda de arco, elástico, motor de pregador, arremessos, recarga, metal, madeira, moedas, gemidos e habilidades. Efeitos de inimigos fora da câmera têm volume reduzido; os avisos visuais identificam a direção sem depender do áudio. Há botão de som no menu.
