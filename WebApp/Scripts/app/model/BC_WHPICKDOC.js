Ext.define('WEBAPP.model.BC_WHPICKDOC', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'PICK_DATE', type: 'date' },
        { name: 'DOCNO', type: 'string' },
        { name: 'APPLY_KIND', type: 'string' },
        { name: 'COMPLEXITY', type: 'string' },
        { name: 'LOT_NO', type: 'string' },


        { name: 'APPID', type: 'string' },
        { name: 'APPITEM_SUM', type: 'string' },
        { name: 'APPQTY_SUM', type: 'string' },

        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});