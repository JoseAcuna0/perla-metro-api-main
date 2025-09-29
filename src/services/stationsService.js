// src/services/stationsService.js

const axios = require('axios');

// Lee la URL base del archivo .env (que debe ser la URL pública de Railway)
const STATIONS_API_BASE = process.env.STATIONS_SERVICE_BASE_URL;

/**
 * Obtiene los detalles de una estación activa (por su ID) desde el Stations Service.
 * @param {number} stationId - ID de la estación a buscar.
 * @returns {object|null} Los datos de la estación o null si no está activa.
 */
async function getStationDetails(stationId) {
    const url = `${STATIONS_API_BASE}/api/stations/${stationId}`; 
    
    try {
        // GET /api/stations/:id no requiere token
        const response = await axios.get(url);
        
        return response.data; 
        
    } catch (error) {
        // Maneja el error 404 (Estación inactiva o no existe)
        if (error.response && error.response.status === 404) {
            console.warn(`[StationsService] Estación ID ${stationId} no encontrada o inactiva.`);
            return null;
        }
        
        console.error("[StationsService] Error de comunicación:", error.message);
        throw new Error("Fallo al contactar al servicio de estaciones.");
    }
}

/**
 * Función para obtener todas las estaciones activas (Requiere token de Admin).
 * @param {string} token - Token de autorización de administrador.
 * @returns {array} Lista de estaciones.
 */
async function getAllActiveStations(token) {
    const url = `${STATIONS_API_BASE}/api/stations`;
    
    try {
        const response = await axios.get(url, {
            headers: {
                // Se asume que la API principal tiene un token válido de Admin
                'Authorization': `Bearer ${token}` 
            }
        });
        return response.data;
    } catch (error) {
        console.error("[StationsService] Error al listar estaciones:", error.message);
        throw new Error("Acceso denegado o fallo del servicio.");
    }
}

module.exports = {
    getStationDetails,
    getAllActiveStations
};