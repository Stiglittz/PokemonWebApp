// ====================================
// SISTEMA DE CACHE CLIENT-SIDE AVANZADO
// ====================================

/**
 * Clase para manejar cache persistente en localStorage
 */
class PokemonCache {
    constructor() {
        this.prefix = 'pokemon_cache_';
        this.version = '1.0'; // Cambiar para invalidar todo el cache
        this.defaultTTL = {
            types: 6 * 60 * 60 * 1000,        // 6 horas
            details: 30 * 60 * 1000,          // 30 minutos  
            species: 60 * 60 * 1000,          // 1 hora
            searches: 15 * 60 * 1000,         // 15 minutos
            filters: 24 * 60 * 60 * 1000      // 24 horas
        };
        
        this.initializeCache();
    }

    /**
     * Inicializa el sistema de cache y limpia datos obsoletos
     */
    initializeCache() {
        try {
            // Verificar si la versión del cache cambió
            const currentVersion = localStorage.getItem(this.prefix + 'version');
            if (currentVersion !== this.version) {
                this.clearAllCache();
                localStorage.setItem(this.prefix + 'version', this.version);
                console.log('🔄 Cache inicializado con nueva versión:', this.version);
            }

            // Limpiar elementos expirados al inicializar
            this.cleanExpiredItems();
            
            console.log('✅ Sistema de cache client-side inicializado');
        } catch (error) {
            console.error('❌ Error inicializando cache:', error);
        }
    }

    /**
     * Obtiene un elemento del cache si no ha expirado
     */
    get(key, category = 'default') {
        try {
            const fullKey = this.prefix + category + '_' + key;
            const item = localStorage.getItem(fullKey);
            
            if (!item) {
                console.log('⚡ Cache MISS:', key);
                return null;
            }

            const parsed = JSON.parse(item);
            const now = Date.now();

            // Verificar si ha expirado
            if (parsed.expires && now > parsed.expires) {
                localStorage.removeItem(fullKey);
                console.log('⏰ Cache EXPIRED:', key);
                return null;
            }

            // Verificar si está próximo a expirar (mostrar indicador visual)
            const timeToExpire = parsed.expires - now;
            const isNearExpiry = timeToExpire < (this.defaultTTL[category] * 0.1); // 10% del TTL

            console.log('✅ Cache HIT:', key, isNearExpiry ? '(próximo a expirar)' : '');
            
            return {
                data: parsed.data,
                timestamp: parsed.timestamp,
                isNearExpiry: isNearExpiry,
                fromCache: true
            };
        } catch (error) {
            console.error('❌ Error obteniendo del cache:', error);
            return null;
        }
    }

    /**
     * Guarda un elemento en el cache con TTL
     */
    set(key, data, category = 'default', customTTL = null) {
        try {
            const fullKey = this.prefix + category + '_' + key;
            const ttl = customTTL || this.defaultTTL[category] || this.defaultTTL.default;
            const now = Date.now();

            const item = {
                data: data,
                timestamp: now,
                expires: now + ttl,
                version: this.version
            };

            localStorage.setItem(fullKey, JSON.stringify(item));
            console.log('💾 Cache STORED:', key, 'TTL:', Math.round(ttl / 1000 / 60), 'min');
            
            return true;
        } catch (error) {
            console.error('❌ Error guardando en cache:', error);
            // Si el localStorage está lleno, limpiar elementos antiguos
            if (error.name === 'QuotaExceededError') {
                this.cleanOldItems();
                // Intentar de nuevo
                try {
                    localStorage.setItem(fullKey, JSON.stringify(item));
                    console.log('💾 Cache STORED (después de limpieza):', key);
                    return true;
                } catch (retryError) {
                    console.error('❌ Error persistente en cache:', retryError);
                }
            }
            return false;
        }
    }

    /**
     * Invalida un elemento específico del cache
     */
    invalidate(key, category = 'default') {
        try {
            const fullKey = this.prefix + category + '_' + key;
            localStorage.removeItem(fullKey);
            console.log('🗑️ Cache INVALIDATED:', key);
        } catch (error) {
            console.error('❌ Error invalidando cache:', error);
        }
    }

    /**
     * Limpia elementos expirados
     */
    cleanExpiredItems() {
        try {
            const now = Date.now();
            let cleanedCount = 0;

            for (let i = localStorage.length - 1; i >= 0; i--) {
                const key = localStorage.key(i);
                if (key && key.startsWith(this.prefix)) {
                    try {
                        const item = JSON.parse(localStorage.getItem(key));
                        if (item.expires && now > item.expires) {
                            localStorage.removeItem(key);
                            cleanedCount++;
                        }
                    } catch (parseError) {
                        // Remover elementos corruptos
                        localStorage.removeItem(key);
                        cleanedCount++;
                    }
                }
            }

            if (cleanedCount > 0) {
                console.log('🧹 Cache limpiado:', cleanedCount, 'elementos expirados');
            }
        } catch (error) {
            console.error('❌ Error limpiando cache expirado:', error);
        }
    }

    /**
     * Limpia elementos antiguos cuando el localStorage está lleno
     */
    cleanOldItems() {
        try {
            const items = [];
            
            // Recolectar todos los elementos del cache con timestamps
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && key.startsWith(this.prefix)) {
                    try {
                        const item = JSON.parse(localStorage.getItem(key));
                        items.push({ key, timestamp: item.timestamp });
                    } catch (parseError) {
                        // Remover elementos corruptos
                        localStorage.removeItem(key);
                    }
                }
            }

            // Ordenar por timestamp (más antiguos primero)
            items.sort((a, b) => a.timestamp - b.timestamp);

            // Remover el 25% más antiguo
            const toRemove = Math.ceil(items.length * 0.25);
            for (let i = 0; i < toRemove; i++) {
                localStorage.removeItem(items[i].key);
            }

            console.log('🧹 Cache: Removidos', toRemove, 'elementos antiguos');
        } catch (error) {
            console.error('❌ Error limpiando elementos antiguos:', error);
        }
    }

    /**
     * Limpia todo el cache
     */
    clearAllCache() {
        try {
            for (let i = localStorage.length - 1; i >= 0; i--) {
                const key = localStorage.key(i);
                if (key && key.startsWith(this.prefix)) {
                    localStorage.removeItem(key);
                }
            }
            console.log('🧹 Todo el cache ha sido limpiado');
        } catch (error) {
            console.error('❌ Error limpiando todo el cache:', error);
        }
    }

    /**
     * Obtiene estadísticas del cache
     */
    getStats() {
        try {
            const stats = {
                totalItems: 0,
                totalSize: 0,
                categories: {},
                nearExpiry: 0
            };

            const now = Date.now();

            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && key.startsWith(this.prefix)) {
                    const value = localStorage.getItem(key);
                    const size = new Blob([value]).size;
                    
                    stats.totalItems++;
                    stats.totalSize += size;

                    // Extraer categoría
                    const category = key.split('_')[2] || 'unknown';
                    if (!stats.categories[category]) {
                        stats.categories[category] = { count: 0, size: 0 };
                    }
                    stats.categories[category].count++;
                    stats.categories[category].size += size;

                    // Verificar si está próximo a expirar
                    try {
                        const item = JSON.parse(value);
                        if (item.expires && (item.expires - now) < (5 * 60 * 1000)) { // 5 minutos
                            stats.nearExpiry++;
                        }
                    } catch (parseError) {
                        // Ignorar elementos corruptos
                    }
                }
            }

            console.log('📊 Cache Stats:', stats);
            return stats;
        } catch (error) {
            console.error('❌ Error obteniendo estadísticas:', error);
            return null;
        }
    }
}

// Instancia global del cache
const pokemonCache = new PokemonCache();

// ====================================
// FUNCIONES MEJORADAS CON CACHE
// ====================================

/**
 * Función mejorada para cargar detalles con cache
 */
async function loadPokemonDetailsWithCache(pokemonId) {
    try {
        // Intentar obtener del cache primero
        const cached = pokemonCache.get(pokemonId.toString(), 'details');
        
        if (cached && cached.data) {
            console.log('📋 Mostrando detalles desde cache para Pokémon:', pokemonId);
            
            // Mostrar indicador de cache si está próximo a expirar
            if (cached.isNearExpiry) {
                showCacheIndicator('Los datos pueden no estar actualizados', 'warning');
            } else {
                showCacheIndicator('Datos cargados instantáneamente', 'success');
            }
            
            populateModal(cached.data);
            return;
        }

        // Si no está en cache, obtener del servidor
        console.log('🌐 Obteniendo detalles desde servidor para Pokémon:', pokemonId);
        showCacheIndicator('Obteniendo datos frescos...', 'info');

        const response = await fetch(`/Pokemon/GetPokemonDetails?id=${pokemonId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result.success && result.pokemon) {
            // Guardar en cache
            pokemonCache.set(pokemonId.toString(), result.pokemon, 'details');
            
            showCacheIndicator('Datos cargados', 'success');
            populateModal(result.pokemon);
        } else {
            throw new Error(result.message || 'No se pudieron cargar los detalles del Pokémon');
        }

    } catch (error) {
        console.error('Error al cargar detalles del Pokémon:', error);
        showModalError(error.message || 'Error de conexión');
    }
}

/**
 * Muestra indicadores visuales del estado del cache
 */
function showCacheIndicator(message, type = 'info') {
    // Remover indicador anterior si existe
    const existingIndicator = document.getElementById('cacheIndicator');
    if (existingIndicator) {
        existingIndicator.remove();
    }

    // Crear nuevo indicador
    const indicator = document.createElement('div');
    indicator.id = 'cacheIndicator';
    indicator.className = `cache-indicator alert alert-${type} position-fixed`;
    indicator.style.cssText = `
        top: 80px; 
        right: 20px; 
        z-index: 9999; 
        max-width: 300px;
        font-size: 0.875rem;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;
    
    const iconMap = {
        success: 'check-circle',
        warning: 'exclamation-triangle', 
        info: 'info-circle',
        danger: 'times-circle'
    };
    
    indicator.innerHTML = `
        <i class="fas fa-${iconMap[type]} me-2"></i>${message}
    `;

    document.body.appendChild(indicator);

    // Fade in
    setTimeout(() => indicator.style.opacity = '1', 100);

    // Auto-remover después de 3 segundos
    setTimeout(() => {
        indicator.style.opacity = '0';
        setTimeout(() => {
            if (indicator.parentNode) {
                indicator.remove();
            }
        }, 300);
    }, 3000);
}

/**
 * Función para limpiar cache manualmente (para debugging)
 */
function clearPokemonCache() {
    pokemonCache.clearAllCache();
    showAlert('Cache limpiado completamente', 'success');
}

/**
 * Función para mostrar estadísticas del cache
 */
function showCacheStats() {
    const stats = pokemonCache.getStats();
    if (stats) {
        const sizeKB = Math.round(stats.totalSize / 1024);
        console.log(`📊 Cache: ${stats.totalItems} elementos, ${sizeKB}KB, ${stats.nearExpiry} próximos a expirar`);
        showAlert(`Cache: ${stats.totalItems} elementos (${sizeKB}KB)`, 'info');
    }
}

/**
 * Cache para filtros aplicados frecuentemente
 */
function cacheFilterState(filters) {
    const filterKey = JSON.stringify(filters);
    pokemonCache.set(filterKey, filters, 'filters');
}

/**
 * Obtiene filtros cacheados
 */
function getCachedFilters(filters) {
    const filterKey = JSON.stringify(filters);
    return pokemonCache.get(filterKey, 'filters');
}

// ====================================
// INTEGRACIÓN CON FUNCIONES EXISTENTES
// ====================================

/**
 * Función mejorada showPokemonDetails que usa cache
 */
function showPokemonDetailsWithCache(pokemonId) {
    if (!pokemonId || pokemonId <= 0) {
        console.error('ID de Pokémon inválido:', pokemonId);
        return;
    }

    currentPokemonId = pokemonId;
    
    // Abrir el modal
    const modal = new bootstrap.Modal(document.getElementById('pokemonDetailsModal'));
    modal.show();
    
    // Resetear el modal al estado de carga
    resetModalToLoading();
    
    // Cargar los datos del Pokémon CON CACHE
    loadPokemonDetailsWithCache(pokemonId);
}

// ====================================
// CSS PARA INDICADORES DE CACHE
// ====================================

/**
 * Inyecta CSS para los indicadores de cache
 */
function injectCacheStyles() {
    const style = document.createElement('style');
    style.textContent = `
        .cache-indicator {
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            border: none;
            animation: slideInRight 0.3s ease;
        }

        @keyframes slideInRight {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }

        .cache-badge {
            position: absolute;
            top: 5px;
            right: 5px;
            background: rgba(40, 167, 69, 0.9);
            color: white;
            font-size: 0.7rem;
            padding: 2px 6px;
            border-radius: 10px;
            z-index: 10;
        }

        .cache-badge.warning {
            background: rgba(255, 193, 7, 0.9);
            color: #212529;
        }

        .near-expiry {
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0% { opacity: 1; }
            50% { opacity: 0.7; }
            100% { opacity: 1; }
        }

        .cache-stats-panel {
            position: fixed;
            bottom: 20px;
            left: 20px;
            background: white;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            font-size: 0.8rem;
            z-index: 1000;
            display: none;
        }
    `;
    document.head.appendChild(style);
}

// ====================================
// INICIALIZACIÓN MEJORADA
// ====================================

/**
 * Inicialización mejorada del sistema con cache
 */
document.addEventListener('DOMContentLoaded', function() {
    // Inyectar estilos
    injectCacheStyles();
    
    // Configurar eventos de checkboxes individuales (mantenido)
    document.querySelectorAll('.pokemon-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            updateSelectedCount();
            
            const selectAllCheckbox = document.getElementById('selectAll');
            const totalCheckboxes = document.querySelectorAll('.pokemon-checkbox').length;
            const checkedCheckboxes = document.querySelectorAll('.pokemon-checkbox:checked').length;
            
            selectAllCheckbox.checked = checkedCheckboxes === totalCheckboxes;
            selectAllCheckbox.indeterminate = checkedCheckboxes > 0 && checkedCheckboxes < totalCheckboxes;
        });
    });

    // Event listeners para emails (mantenido)
    const sendEmailBtn = document.getElementById('sendEmailBtn');
    if (sendEmailBtn) {
        sendEmailBtn.addEventListener('click', sendEmail);
    }

    // Validación en tiempo real del email (mantenido)
    const emailInput = document.getElementById('recipientEmail');
    if (emailInput) {
        emailInput.addEventListener('input', function() {
            const isValid = this.checkValidity();
            this.classList.toggle('is-valid', isValid && this.value.length > 0);
            this.classList.toggle('is-invalid', !isValid && this.value.length > 0);
        });
    }

    // NUEVO: Limpiar cache expirado cada 10 minutos
    setInterval(() => {
        pokemonCache.cleanExpiredItems();
    }, 10 * 60 * 1000);

    // NUEVO: Mostrar estadísticas del cache en desarrollo
    if (window.location.hostname === 'localhost') {
        setTimeout(() => {
            const stats = pokemonCache.getStats();
            if (stats && stats.totalItems > 0) {
                console.log('🎯 Cache inicializado con', stats.totalItems, 'elementos');
            }
        }, 2000);
    }

    // Configuración inicial (mantenida)
    updateSelectedCount();
    
    console.log('🚀 Sistema de exportación a Excel inicializado correctamente');
    console.log('🎯 Modal de detalles de Pokémon con CACHE inicializado correctamente');
    console.log('💾 Sistema de cache client-side inicializado correctamente');
});

// ====================================
// FUNCIONES DE DESARROLLO/DEBUG
// ====================================

/**
 * Funciones disponibles en consola para debugging
 */
window.pokemonCacheDebug = {
    showStats: showCacheStats,
    clearCache: clearPokemonCache,
    getCache: () => pokemonCache,
    testCache: () => {
        // Prueba básica del cache
        pokemonCache.set('test', { data: 'test' }, 'details');
        const result = pokemonCache.get('test', 'details');
        console.log('✅ Test cache:', result ? 'PASSED' : 'FAILED');
        pokemonCache.invalidate('test', 'details');
    }
};

console.log('🔧 Debug functions available: window.pokemonCacheDebug');