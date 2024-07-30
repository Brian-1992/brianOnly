Ext.define('WEBAPP.model.WB_AIRHIS', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'AGEN_NO', type: 'string' },
        { name: 'FBNO', type: 'string' },
        { name: 'NAMEC', type: 'string' },        
        { name: 'SEQ', type: 'int' },
        { name: 'TXTDAY', type: 'date' },
        { name: 'XSIZE', type: 'string' },

        { name: 'CREATE_TIME', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'EXTYPE', type: 'string' },
        { name: 'STATUS', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' }
    ]
});