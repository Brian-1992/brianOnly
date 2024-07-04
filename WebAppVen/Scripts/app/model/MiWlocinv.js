Ext.define('WEBAPP.model.MiWlocinv', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'STORE_LOC', type: 'string' },
        { name: 'LOC_NAME', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }/*,
        { name: 'MMNAME_E', type: 'string' }*/
    ]
});