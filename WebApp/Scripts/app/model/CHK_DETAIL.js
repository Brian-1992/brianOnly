Ext.define('WEBAPP.model.CHK_DETAIL', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'CHK_NO', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        //{ name: 'M_PURUN', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'M_CONTPRICE', type: 'string' },
        { name: 'WH_NO', type: 'string' },
        { name: 'STORE_LOC', type: 'string' },
        { name: 'LOC_NAME', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'STORE_QTYC', type: 'string' },
        { name: 'STORE_QTYM', type: 'string' },
        { name: 'STORE_QTYS', type: 'string' },
        { name: 'CHK_QTY', type: 'string' },
        { name: 'CHK_REMARK', type: 'string' },
        { name: 'CHK_UID', type: 'string' },
        { name: 'CHK_TIME', type: 'string' },
        { name: 'STATUS_INI', type: 'string' },


        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },

        { name: 'DONE_NUM', type: 'string' },
        { name: 'UNDONE_NUM', type: 'string' },
        { name: 'DONE_STATUS', type: 'string' },
        { name: 'CHK_UID_NAME', type: 'string' },
    ]
});