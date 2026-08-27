shader_type canvas_item;

// Controla a velocidade do movimento nos eixos X e Y
uniform vec2 velocidade = vec2(0.05, 0.05);

void fragment() {
    // Desloca as coordenadas UV com base no tempo do jogo
    vec2 uv_animado = UV + (TIME * velocidade);
    COLOR = texture(TEXTURE, uv_animado);
}