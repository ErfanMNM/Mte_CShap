// API Base URL
const API_BASE = '/api';

// State
let currentPage = 'releases';

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    initModals();
    initForms();
    loadPage('releases');
});

// Navigation
function initNavigation() {
    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const page = item.dataset.page;
            loadPage(page);
        });
    });
}

function loadPage(page) {
    currentPage = page;
    
    // Update nav
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.toggle('active', item.dataset.page === page);
    });
    
    // Update pages
    document.querySelectorAll('.page').forEach(p => {
        p.classList.toggle('active', p.id === `page-${page}`);
    });
    
    // Update title
    const titles = {
        releases: 'Releases',
        clients: 'Clients',
        backups: 'Backups',
        events: 'Events',
        stats: 'Statistics'
    };
    document.getElementById('page-title').textContent = titles[page] || 'Dashboard';
    
    // Load data
    switch(page) {
        case 'releases':
            loadReleases();
            break;
        case 'clients':
            loadClients();
            break;
        case 'backups':
            loadBackups();
            break;
        case 'events':
            loadEvents();
            break;
        case 'stats':
            loadStats();
            break;
    }
}

// Modals
function initModals() {
    // Close buttons
    document.querySelectorAll('.modal-close, [data-modal]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            if (e.target.classList.contains('btn-secondary') || e.target.closest('.modal-close')) {
                const modalId = btn.dataset.modal || btn.closest('[data-modal]')?.dataset.modal;
                if (modalId) {
                    closeModal(modalId);
                }
            }
        });
    });
    
    // New release button
    document.getElementById('btn-new-release').addEventListener('click', () => {
        openModal('modal-new-release');
    });
    
    // Refresh buttons
    document.getElementById('btn-refresh')?.addEventListener('click', loadReleases);
    document.getElementById('btn-refresh-clients')?.addEventListener('click', loadClients);
    document.getElementById('btn-refresh-backups')?.addEventListener('click', loadBackups);
    document.getElementById('btn-refresh-events')?.addEventListener('click', loadEvents);
}

function openModal(modalId) {
    document.getElementById(modalId).classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

// Forms
function initForms() {
    const form = document.getElementById('form-new-release');
    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await submitNewRelease(form);
        });
    }
}

async function submitNewRelease(form) {
    const formData = new FormData(form);
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    try {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Uploading...';
        
        const response = await fetch(`${API_BASE}/releases`, {
            method: 'POST',
            body: formData
        });
        
        const data = await response.json();
        
        if (data.success) {
            showToast('Release created successfully!', 'success');
            form.reset();
            closeModal('modal-new-release');
            loadReleases();
        } else {
            showToast(data.error || 'Failed to create release', 'error');
        }
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// Load Releases
async function loadReleases() {
    const tbody = document.getElementById('releases-table-body');
    if (!tbody) return;
    
    tbody.innerHTML = '<tr><td colspan="7" class="loading">Loading...</td></tr>';
    
    try {
        const params = new URLSearchParams();
        const product = document.getElementById('filter-product')?.value;
        const channel = document.getElementById('filter-channel')?.value;
        const os = document.getElementById('filter-os')?.value;
        
        if (product) params.append('product', product);
        if (channel) params.append('channel', channel);
        if (os) params.append('os', os);
        
        const response = await fetch(`${API_BASE}/releases?${params}`);
        const data = await response.json();
        
        if (data.success && data.releases) {
            renderReleases(data.releases);
        } else {
            tbody.innerHTML = '<tr><td colspan="7" class="loading">No releases found</td></tr>';
        }
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="7" class="loading">Error: ${error.message}</td></tr>`;
    }
}

function renderReleases(releases) {
    const tbody = document.getElementById('releases-table-body');
    
    if (releases.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="loading">No releases found</td></tr>';
        return;
    }
    
    tbody.innerHTML = releases.map(release => `
        <tr>
            <td>${release.id}</td>
            <td><strong>${release.product}</strong></td>
            <td><span class="badge badge-info">${release.version}</span></td>
            <td><span class="badge badge-${getChannelBadge(release.channel)}">${release.channel}</span></td>
            <td>${release.os}/${release.arch}</td>
            <td>${formatDate(release.publishedAt)}</td>
            <td>
                <button class="btn btn-sm btn-secondary" onclick="viewRelease(${release.id})">
                    <i class="fas fa-eye"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteRelease(${release.id})">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

// Load Clients
async function loadClients() {
    const tbody = document.getElementById('clients-table-body');
    tbody.innerHTML = '<tr><td colspan="7" class="loading">Loading...</td></tr>';
    
    try {
        const params = new URLSearchParams();
        const product = document.getElementById('filter-client-product')?.value;
        const site = document.getElementById('filter-client-site')?.value;
        const status = document.getElementById('filter-client-status')?.value;
        
        if (product) params.append('product', product);
        if (site) params.append('site', site);
        if (status) params.append('status', status);
        
        const response = await fetch(`${API_BASE}/clients?${params}`);
        const data = await response.json();
        
        if (data.success && data.clients) {
            renderClients(data.clients);
        } else {
            tbody.innerHTML = '<tr><td colspan="7" class="loading">No clients found</td></tr>';
        }
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="7" class="loading">Error: ${error.message}</td></tr>`;
    }
}

function renderClients(clients) {
    const tbody = document.getElementById('clients-table-body');
    
    if (clients.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="loading">No clients found</td></tr>';
        return;
    }
    
    tbody.innerHTML = clients.map(client => `
        <tr>
            <td><code>${client.clientId}</code></td>
            <td>${client.product}</td>
            <td>${client.currentVersion}</td>
            <td>${client.os}/${client.arch}</td>
            <td>${client.site || '-'}</td>
            <td>${formatDate(client.lastSeen)}</td>
            <td><span class="badge badge-${getStatusBadge(client.status)}">${client.status}</span></td>
        </tr>
    `).join('');
}

// Load Backups
async function loadBackups() {
    const tbody = document.getElementById('backups-table-body');
    tbody.innerHTML = '<tr><td colspan="7" class="loading">Loading...</td></tr>';
    
    try {
        const clientId = document.getElementById('filter-backup-client')?.value;
        const product = document.getElementById('filter-backup-product')?.value;
        
        if (!clientId) {
            tbody.innerHTML = '<tr><td colspan="7" class="loading">Please enter a Client ID</td></tr>';
            return;
        }
        
        const params = new URLSearchParams();
        if (product) params.append('product', product);
        
        const response = await fetch(`${API_BASE}/backups/${clientId}?${params}`);
        const data = await response.json();
        
        if (data.success && data.backups) {
            renderBackups(data.backups);
        } else {
            tbody.innerHTML = '<tr><td colspan="7" class="loading">No backups found</td></tr>';
        }
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="7" class="loading">Error: ${error.message}</td></tr>`;
    }
}

function renderBackups(backups) {
    const tbody = document.getElementById('backups-table-body');
    
    if (backups.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="loading">No backups found</td></tr>';
        return;
    }
    
    tbody.innerHTML = backups.map(backup => `
        <tr>
            <td><code>${backup.backupId}</code></td>
            <td><code>${backup.clientId}</code></td>
            <td>${backup.product}</td>
            <td>${backup.version}</td>
            <td>${formatBytes(backup.size)}</td>
            <td>${formatDate(backup.createdAt)}</td>
            <td>
                <a href="${backup.downloadUrl}" target="_blank" class="btn btn-sm btn-secondary">
                    <i class="fas fa-download"></i>
                </a>
                <button class="btn btn-sm btn-danger" onclick="deleteBackup('${backup.backupId}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

// Load Events
async function loadEvents() {
    const tbody = document.getElementById('events-table-body');
    tbody.innerHTML = '<tr><td colspan="6" class="loading">Loading...</td></tr>';
    
    try {
        const params = new URLSearchParams();
        const clientId = document.getElementById('filter-event-client')?.value;
        const product = document.getElementById('filter-event-product')?.value;
        const type = document.getElementById('filter-event-type')?.value;
        
        if (clientId) params.append('clientId', clientId);
        if (product) params.append('product', product);
        if (type) params.append('type', type);
        
        const response = await fetch(`${API_BASE}/events?${params}`);
        const data = await response.json();
        
        if (data.success && data.events) {
            renderEvents(data.events);
        } else {
            tbody.innerHTML = '<tr><td colspan="6" class="loading">No events found</td></tr>';
        }
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="6" class="loading">Error: ${error.message}</td></tr>`;
    }
}

function renderEvents(events) {
    const tbody = document.getElementById('events-table-body');
    
    if (events.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="loading">No events found</td></tr>';
        return;
    }
    
    tbody.innerHTML = events.map(event => `
        <tr>
            <td>${event.id}</td>
            <td><code>${event.clientId}</code></td>
            <td><span class="badge badge-${getEventBadge(event.type)}">${event.type}</span></td>
            <td>${event.product}</td>
            <td>${event.version || '-'}</td>
            <td>${formatDate(event.timestamp)}</td>
        </tr>
    `).join('');
}

// Load Stats
async function loadStats() {
    try {
        // Load releases count - get total count
        const releasesRes = await fetch(`${API_BASE}/releases?limit=1000`);
        const releasesData = await releasesRes.json();
        const releasesCount = releasesData.releases?.length || 0;
        const statReleases = document.getElementById('stat-releases');
        if (statReleases) statReleases.textContent = releasesCount;
        
        // Load clients count
        const clientsRes = await fetch(`${API_BASE}/clients?status=active&limit=1000`);
        const clientsData = await clientsRes.json();
        const clientsCount = clientsData.clients?.length || 0;
        const statClients = document.getElementById('stat-clients');
        if (statClients) statClients.textContent = clientsCount;
        
        // Load backups count (need to aggregate - simplified)
        const statBackups = document.getElementById('stat-backups');
        if (statBackups) statBackups.textContent = '-';
        
        // Load events count (24h)
        const eventsRes = await fetch(`${API_BASE}/events?limit=1000`);
        const eventsData = await eventsRes.json();
        const last24h = eventsData.events?.filter(e => {
            const eventDate = new Date(e.timestamp);
            const now = new Date();
            return (now - eventDate) < 24 * 60 * 60 * 1000;
        }).length || 0;
        const statEvents = document.getElementById('stat-events');
        if (statEvents) statEvents.textContent = last24h;
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

// Actions
async function viewRelease(id) {
    try {
        const response = await fetch(`${API_BASE}/releases/${id}`);
        const data = await response.json();
        
        if (data.success && data.release) {
            const release = data.release;
            const content = `
                <div class="release-details">
                    <div class="detail-row">
                        <strong>Product:</strong> ${release.product}
                    </div>
                    <div class="detail-row">
                        <strong>Version:</strong> <span class="badge badge-info">${release.version}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Channel:</strong> <span class="badge badge-${getChannelBadge(release.channel)}">${release.channel}</span>
                    </div>
                    <div class="detail-row">
                        <strong>OS/Arch:</strong> ${release.os}/${release.arch}
                    </div>
                    <div class="detail-row">
                        <strong>Published:</strong> ${formatDate(release.publishedAt)}
                    </div>
                    ${release.changelog ? `<div class="detail-row"><strong>Changelog:</strong><br>${release.changelog}</div>` : ''}
                    ${release.filePassword ? `<div class="detail-row"><strong>File Password:</strong> <code>${release.filePassword}</code></div>` : ''}
                    <div class="detail-row">
                        <strong>Download:</strong> 
                        <a href="${release.downloadUrl}" target="_blank" class="btn btn-sm btn-primary">
                            <i class="fas fa-download"></i> Download
                        </a>
                    </div>
                </div>
            `;
            document.getElementById('release-details-content').innerHTML = content;
            openModal('modal-release-details');
        }
    } catch (error) {
        showToast('Error loading release details', 'error');
    }
}

async function deleteRelease(id) {
    if (!confirm('Are you sure you want to delete this release?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/releases/${id}`, {
            method: 'DELETE'
        });
        
        const data = await response.json();
        
        if (data.success) {
            showToast('Release deleted successfully', 'success');
            loadReleases();
        } else {
            showToast(data.error || 'Failed to delete release', 'error');
        }
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
    }
}

async function deleteBackup(backupId) {
    if (!confirm('Are you sure you want to delete this backup?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/backups/${backupId}`, {
            method: 'DELETE'
        });
        
        const data = await response.json();
        
        if (data.success) {
            showToast('Backup deleted successfully', 'success');
            loadBackups();
        } else {
            showToast(data.error || 'Failed to delete backup', 'error');
        }
    } catch (error) {
        showToast('Error: ' + error.message, 'error');
    }
}

// Utilities
function formatDate(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN');
}

function formatBytes(bytes) {
    if (!bytes) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

function getChannelBadge(channel) {
    const badges = {
        stable: 'success',
        beta: 'warning',
        dev: 'info'
    };
    return badges[channel] || 'info';
}

function getStatusBadge(status) {
    const badges = {
        active: 'success',
        inactive: 'warning',
        error: 'danger'
    };
    return badges[status] || 'info';
}

function getEventBadge(type) {
    const badges = {
        check: 'info',
        download: 'info',
        backup_ok: 'success',
        install_ok: 'success',
        health_fail: 'danger',
        rollback: 'warning',
        error: 'danger'
    };
    return badges[type] || 'info';
}

function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
        <span>${message}</span>
    `;
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideIn 0.3s ease reverse';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Make functions global
window.viewRelease = viewRelease;
window.deleteRelease = deleteRelease;
window.deleteBackup = deleteBackup;

