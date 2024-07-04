Ext.define('WEBAPP.model.BC_WHCHKID', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'WH_CHKUID', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },

        { name: 'ITEM_STRING', type: 'string' },

        { name: 'WH_CHKUID_NAME', type: 'string' },
        { name: 'WH_KIND', type: 'string' },
        { name: 'WH_GRADE', type: 'string' },
        { name: 'WH_NAME', type: 'string' },
    ]
});