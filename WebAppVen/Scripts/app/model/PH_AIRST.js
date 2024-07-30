Ext.define('WEBAPP.model.PH_AIRST', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'AGEN_NO', type: 'string' },
        { name: 'FBNO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'SEQ', type: 'int' },
        { name: 'TXTDAY', type: 'date' },
        { name: 'AIR', type: 'string' },
        { name: 'DEPT', type: 'string' },
        { name: 'XSIZE', type: 'string' },

        { name: 'CREATE_TIME', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'EXTYPE', type: 'string' },
        { name: 'STATUS', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' }
    ]
});