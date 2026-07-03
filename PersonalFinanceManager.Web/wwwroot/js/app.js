// ── Local Storage helpers ──────────────────────────────
window.localStorageHelper = {
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    },
    clear: function () {
        localStorage.clear();
    }
};

// ── Chart.js helpers ───────────────────────────────────
// Each helper destroys any existing chart on the canvas before rendering a new one
// so that Blazor re-renders (OnAfterRenderAsync) don't stack duplicate charts.
window.chartHelper = {
    _charts: {},

    _destroy: function (canvasId) {
        if (window.chartHelper._charts[canvasId]) {
            window.chartHelper._charts[canvasId].destroy();
            delete window.chartHelper._charts[canvasId];
        }
    },

    /** Doughnut chart — spending by category */
    renderDoughnut: function (canvasId, labels, data, backgroundColors) {
        window.chartHelper._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        window.chartHelper._charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'right' },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                const pct = ctx.dataset.data[ctx.dataIndex];
                                return ` ${ctx.label}: ${pct.toFixed(1)}%`;
                            }
                        }
                    }
                }
            }
        });
    },

    /** Bar chart — income vs expenses (pass month labels + two value arrays) */
    renderIncomeExpenseBar: function (canvasId, labels, incomeData, expenseData) {
        window.chartHelper._destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        window.chartHelper._charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Income',
                        data: incomeData,
                        backgroundColor: 'rgba(25, 135, 84, 0.7)',
                        borderColor: 'rgba(25, 135, 84, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Expenses',
                        data: expenseData,
                        backgroundColor: 'rgba(220, 53, 69, 0.7)',
                        borderColor: 'rgba(220, 53, 69, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'top' } },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) { return '$' + value.toLocaleString(); }
                        }
                    }
                }
            }
        });
    }
};
