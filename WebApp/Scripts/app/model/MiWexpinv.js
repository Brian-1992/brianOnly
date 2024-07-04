Ext.define('WEBAPP.model.MiWexpinv', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'EXP_DATE', type: 'string' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'INVQTY', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});