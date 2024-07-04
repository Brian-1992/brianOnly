Ext.define('WEBAPP.model.MI_UNITEXCH', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' },
        { name: 'UNIT_CODE', type: 'string' },
        { name: 'AGEN_NO', type: 'string' },
        { name: 'EXCH_RATIO', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },

        { name: 'MMNAME_C', type: 'string' },
        { name: 'UI_CHANAME', type: 'string' },
        { name: 'AGEN_NAMEC', type: 'string' }
    ]
});