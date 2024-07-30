Ext.define('WEBAPP.model.MI_UNITCODE', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'UNIT_CODE', type: 'string' },
        { name: 'UI_CHANAME', type: 'string' },
        { name: 'UI_ENGNAME', type: 'string' },
        { name: 'UI_SNAME', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },

        // =================== 湘倫 ======================
        { name: 'UI_NAME', type: 'string' },     // 自訂 UNIT_CODE + UI_CHANAME
        // ===============================================
    ]
});