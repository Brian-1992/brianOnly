Ext.define('WEBAPP.model.ME_MDFM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MDFM', type: 'string' },
        { name: 'MD_NAME', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'MDFM_QTY', type: 'string' },
        { name: 'MDFM_UNIT', type: 'string' },
        { name: 'USE_QTY', type: 'string' },
        { name: 'PRESERVE_DAYS', type: 'string' },
        { name: 'OPERATION', type: 'string' },
        { name: 'ELEMENTS', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' }, 
        { name: 'CREATE_ID', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_ID', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },             
        //==========
        { name: 'DOCNO', type: 'string' },
        { name: 'DOCTYPE', type: 'string' },
        { name: 'FLOWID', type: 'string' },
        { name: 'APPID', type: 'string' },
        { name: 'APPDEPT', type: 'string' },
        { name: 'APPTIME', type: 'string' },
        { name: 'USEID', type: 'string' },
        { name: 'USEDEPT', type: 'string' },
        { name: 'FRWH', type: 'string' },
        { name: 'TOWH', type: 'string' },
        { name: 'LIST_ID', type: 'string' },
        { name: 'APPLY_KIND', type: 'string' },
        { name: 'APPLY_NOTE', type: 'string' },
        //{ name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        //{ name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        //{ name: 'UPDATE_IP', type: 'string' },

        { name: 'FLOWID_N', type: 'string' },
        { name: 'MAT_CLASS_N', type: 'string' },
        { name: 'FRWH_N', type: 'string' },
        { name: 'TOWH_N', type: 'string' },
        { name: 'APPLY_KIND_N', type: 'string' },
        { name: 'APPTIME_T', type: 'string' },

        // =================== 紹朋 ======================
        { name: 'APPTIME_TWN', type: 'string' },
        // ===============================================

        // =================== 湘倫 ======================
        { name: 'APP_NAME', type: 'string' },
        { name: 'APPDEPT_NAME', type: 'string' },
        { name: 'USEDEPT_NAME', type: 'string' },
        { name: 'FRWH_NAME', type: 'string' },
        { name: 'TOWH_NAME', type: 'string' }
        // ===============================================
    ]
});