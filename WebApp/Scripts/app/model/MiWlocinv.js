Ext.define('WEBAPP.model.MiWlocinv', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'STORE_LOC', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'CHECK_RESULT', type: 'string' },
        { name: 'IMPORT_RESULT', type: 'string' },
        { name: 'WH_NAME_DISPLAY', type: 'string' },
        { name: 'WH_NAME_TEXT', type: 'string' },
        { name: 'MMCODE_DISPLAY', type: 'string' },
        { name: 'MMCODE_TEXT', type: 'string' },
        { name: 'STORE_LOC_DISPLAY', type: 'string' },
        { name: 'STORE_LOC_TEXT', type: 'string' },
        { name: 'LOC_NOTE', type: 'string' },
        { name: 'LOC_NOTE_TEXT', type: 'string' }
    ]
});