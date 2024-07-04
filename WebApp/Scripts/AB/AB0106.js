Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // 拆解網址參數
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var use_type = Ext.getUrlParam('use_type');
    
    var isCancel = false;

    // 部門
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        listeners: {
            filterchange: function (store, filters, eOpts) {
                
            }
        }
    });

    function setWhnoStore() {
        Ext.Ajax.request({
            url: '/api/AB0106/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                whnoQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            whnoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                    else
                        Ext.Msg.alert('訊息', '點收人員尚無對應部門資料');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWhnoStore();

    function getBarcodeRecord() {
        msglabel('');
        Ext.Ajax.request({
            url: '/api/AB0106/GetBarcodeRecord',
            method: reqVal_p,
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue(),
                barcode: T1Query.getForm().findField('P1').getValue(),
                mmcode: T1Query.getForm().findField('P2').getValue(),
                use_type: use_type
            },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                
                if (data.success) {
                    if (data.etts.length == 0) {
                        Ext.Msg.alert('提示', '查無資料，請重新輸入');
                        return;
                    }
                    var returnData = data.etts[0];
                    
                    setItemDetail(returnData);

                    changeCardItemDetail();

                } else {
                    Ext.Msg.alert('錯誤', data.msg);
                    if (T1Query.getForm().findField('P1_SEL').getValue() == true) {
                        T1Query.getForm().findField('P1').setValue('');
                        T1Query.getForm().findField('P1').focus()
                    } else {
                        T1Query.getForm().findField('P2').setValue('');
                        T1Query.getForm().findField('P2').focus()
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function setItemDetail(data) {
        var f = itemDetail.getForm();
        f.findField('WH_NO').setValue(T1Query.getForm().findField('P0').getValue());
        f.findField('MMCODE').setValue(data.MMCODE);
        f.findField('MMNAME_C').setValue(data.MMNAME_C);
        f.findField('MMNAME_E').setValue(data.MMNAME_E);
        f.findField('BASE_UNIT').setValue(data.BASE_UNIT);
        f.findField('M_TRNID').setValue(data.M_TRNID);
        f.findField('E_SOURCECODE').setValue(data.E_SOURCECODE);
        f.findField('CANCEL_ID').setValue(data.CANCEL_ID);
        f.findField('MAT_CLASS').setValue(data.MAT_CLASS);
        f.findField('WEXP_ID').setValue(data.WEXP_ID);
        f.findField('INV_QTY').setValue(data.INV_QTY);
        f.findField('LOT_NO').setValue(data.LOT_NO);
        f.findField('LOT_NO_UDI').setValue(data.LOT_NO);
        f.findField('EXP_DATE').setValue(data.EXP_DATE);
        f.findField('EXP_DATE_UDI').setValue(data.EXP_DATE_UDI);
        f.findField('TRATIO').setValue(data.TRATIO);
        f.findField('BARCODE').setValue(data.BARCODE);
        f.findField('IS_UDI').setValue(data.IS_UDI);

        f.findField('LOT_NO').show();
        f.findField('LOT_NO_UDI').show();
        f.findField('EXP_DATE').show();
        f.findField('EXP_DATE_UDI').show();

        f.findField('ACKTIMES').setValue('');
        f.findField('ADJQTY').setValue('');
        f.findField('SUSE_NOTE').setValue('');

        if (data.IS_UDI == 'Y') {
            f.findField('LOT_NO').hide();
            f.findField('EXP_DATE').hide();
        } else {
            f.findField('LOT_NO_UDI').hide();
            f.findField('EXP_DATE_UDI').hide();
        }
    }

    function setData() {
        var f = itemDetail.getForm();
        
        if (f.findField('WEXP_ID').getValue() == 'Y' && f.findField('IS_UDI') != 'Y') {
            if (f.findField('LOT_NO').getValue() == null || f.findField('LOT_NO').getValue() == '' ||
                f.findField('EXP_DATE').getValue() == null || f.findField('EXP_DATE').getValue() == '') {
                Ext.Msg.alert('提示', '請填入批號效期');
                return;
            }
        }
        if (f.findField('ACKTIMES').getValue() == null || f.findField('ACKTIMES').getValue() == '') {
            f.findField('ACKTIMES').setValue('0');
        }
        if (f.findField('ADJQTY').getValue() == null || f.findField('ADJQTY').getValue() == '') {
            f.findField('ADJQTY').setValue('0');
        }

        if (Number(f.findField('ACKTIMES').getValue()) < 0 || Number(f.findField('ADJQTY').getValue()) < 0) {
            Ext.Msg.alert('提示', '不可輸入負值，請重新確認');
            return;
        }

        myMask.show();
        
        Ext.Ajax.request({
            url: '/api/AB0106/SetData',
            method: reqVal_p,
            params: {
                wh_no: f.findField('WH_NO').getValue(),
                mmcode: f.findField('MMCODE').getValue(),
                use_type: use_type,
                tratio: f.findField('TRATIO').getValue(),
                acktimes: f.findField('ACKTIMES').getValue(),
                adjqty: f.findField('ADJQTY').getValue(),
                base_unit: f.findField('BASE_UNIT').getValue(),
                bf_invqty: f.findField('INV_QTY').getValue(),
                wexp_id: f.findField('WEXP_ID').getValue(),
                lot_no: f.findField('LOT_NO').getValue(),
                exp_date: f.findField('EXP_DATE').rawValue,
                m_trnid: f.findField('M_TRNID').getValue(),
                e_sourcecode: f.findField('E_SOURCECODE').getValue(),
                scan_barcode: f.findField('BARCODE').getValue(),
                suse_note: f.findField('SUSE_NOTE').getValue(),
                exp_date_udi: f.findField('EXP_DATE_UDI').getValue(),

            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    myMask.hide();
                    Ext.Msg.alert('提示', data.msg);
                    return;
                }
                f.findField('EXP_DATE').blur();
                f.findField('EXP_DATE').collapse();
                if (use_type == 'A') {
                    msglabel('扣庫成功');
                }
                if (use_type == 'B') {
                    msglabel('繳回成功');
                }
                myMask.hide();
                changeCardQuery();
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    //#region card change
    function changeCardQuery() {
        T1Query.getForm().findField('P1').setValue('');
        T1Query.getForm().findField('P2').setValue('');
        setTimeout(function () {
            var layoutG = viewport.down('#t1Card').getLayout();
            layoutG.setActiveItem(0);
        }, 100);
    }

    function changeCardItemDetail() {
        setTimeout(function () {
            var layout = viewport.down('#t1Card').getLayout();
            layout.setActiveItem(1);
        }, 100);
    }
    //#endregion

    // 依傳入的欄位是P1(條碼)或P2(院內碼)設定相關欄位屬性
    function enableField(fieldType) {
        var f = T1Query.getForm();
        if (fieldType == 'P1') {
            f.findField('P1_SEL').setValue(true);
            f.findField('P2_SEL').setValue(false);
            f.findField('P2').setValue('');
            f.findField('P1').setDisabled(false);
            f.findField('P2').setDisabled(true);
            Ext.getCmp('scanBtn').enable();
            Ext.getCmp('query').disable();
            Ext.getCmp('clear').disable();
        }
        else if (fieldType == 'P2') {
            f.findField('P2_SEL').setValue(true);
            f.findField('P1_SEL').setValue(false);
            f.findField('P1').setValue('');
            f.findField('P2').setDisabled(false);
            f.findField('P1').setDisabled(true);
            Ext.getCmp('scanBtn').disable();
            Ext.getCmp('query').enable();
            Ext.getCmp('clear').enable();
        }
    }

    var T1Query = Ext.widget({
        itemId: 'form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 25%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    id: 'P0',
                    name: 'P0',
                    fieldCls: 'required',
                    allowBlank: false,
                    margin: '1 0 1 0',
                    fieldLabel: '庫房',
                    queryMode: 'local',
                    labelStyle: 'width: 30%',
                    triggerAction: 'all',
                    autoSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {
                            
                            setTimeout(function () {
                                T1Query.getForm().findField('P1').focus();
                            }, 300);

                        },
                        focus: function (field, event, eOpts) {
                            
                            //field.getStore().reset();
                            if (isCancel == true) {
                                isCancel = false;
                                T1Query.getForm().findField('P1').focus();
                                return;
                            }
                            field.reset('');
                            // 未處理問題：若有篩選過，expand顯示已篩選的值，但實際上選到的是清空filter的值
                            if (!field.isExpanded) {
                                
                                setTimeout(function () {
                                    field.expand();
                                }, 500);
                            }
                        },
                        beforequery: function (queryPlam, eOpts) {
                            
                        }
                    }
                },
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [
                        {
                            xtype: 'radiofield',
                            id: 'P1_SEL',
                            name: 'P1_SEL',
                            width: '1%',
                            padding: '4% 0 0 4%',
                            checked: true,
                            listeners: {
                                change: function (field, nVal, oVal, eOpts) {
                                    if (nVal) {
                                        enableField('P1');
                                    }
                                }
                            }
                        }, 
                        {
                            xtype: 'textfield',
                            fieldLabel: '條碼',
                            name: 'P1',
                            id: 'P1',
                            width: '90%',
                            labelWidth: false,
                            labelStyle: 'width: 32%',
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length > 7) {
                                        //chkBarcode(nValue);
                                        getBarcodeRecord();
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            id:'scanBtn',
                            iconCls: 'TRACamera',
                            width: '9%',
                            handler: function () {
                                showScanWin(viewport);
                            }
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [
                        {
                            xtype: 'radiofield',
                            id: 'P2_SEL',
                            name: 'P2_SEL',
                            width: '1%',
                            padding: '4% 0 0 4%',
                            listeners: {
                                change: function (field, nVal, oVal, eOpts) {
                                    if (nVal) {
                                        enableField('P2');
                                    }
                                }
                            }
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'P2',
                            id: 'P2',
                            margin: '1 0 1 0',
                            enforceMaxLength: true,
                            disabled:true,
                            labelStyle: 'width: 30%',
                            width: '95%',
                        },
                    ]
                },
                
                {
                    xtype: 'panel',
                    border: false,
                    plugins: 'responsive',
                    margin: '1 0 1 0',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'hbox',
                            }
                        },
                        'width >= 600': {
                            layout: {
                                type: 'hbox',
                                vertical: false
                            }
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        id:'query',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var f = T1Query.getForm();
                            if (!f.findField('P0').getValue()) {
                                Ext.Msg.alert('提示', '庫房為必填');
                                return;
                            }
                            if (!f.findField('P1').getValue() && !f.findField('P2').getValue()) {
                                Ext.Msg.alert('提示', '條碼或院內碼需擇一輸入');
                                return;
                            }

                            getBarcodeRecord();

                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        id:'clear',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').reset();
                            //f.findField('P0').setValue('');
                            //f.findField('P1').setValue('');
                            //f.findField('P2').setValue('');
                            enableField('P1');
                            msglabel('');
                        }
                    }]
                }
            ]
        }]
    });

    var itemDetail = Ext.widget({
        id: 'itemDetail',
        height: '100%',
        xtype: 'form',
        layout: 'form',
        closable: false,
        border: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            //labelAlign: 'right',
            //labelWidth: 90,
            width: '100%',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 35%',
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                //padding: 0,
                //margin: 0,
                items: [
                    {
                        xtype: 'hidden',
                        //fieldLabel: '院內碼',
                        name: 'WH_NO',
                        readOnly: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        readOnly: true
                    },
                    {
                        xtype: 'hidden',
                        name:'M_TRNID'
                    },
                    {
                        xtype: 'hidden',
                        name: 'IS_UDI'
                    },
                    {
                        xtype: 'hidden',
                        name: 'E_SOURCECODE'
                    },
                    {
                        xtype: 'hidden',
                        name: 'CANCEL_ID'
                    },
                    {
                        xtype: 'hidden',
                        name: 'MAT_CLASS'
                    },
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        width: '100%',
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '',
                                id:'tratio_name',
                                name: 'TRATIO',
                                labelStyle: 'width: 55%',
                                width: '63%'
                            }, {
                                xtype: 'numberfield',
                                fieldLabel: '×',
                                labelSeparator: '',
                                name: 'ACKTIMES',
                                readOnly: false,
                                selectOnFocus: true,
                                labelStyle: 'width: 30%',
                                width: '32%',
                                hideTrigger: true,
                                minValue: 0,
                                allowDecimals:false
                                //maskRe: /[0-9.]/,
                                //regexText: '只能輸入數字',
                                //regex: /^0|[1-9]\d*$/
                            }
                        ]
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '+',
                        labelSeparator: '',
                        name: 'ADJQTY',
                        readOnly: false,
                        selectOnFocus: true,
                        labelWidth: false,
                        hideTrigger: true,
                        minValue: 0,
                        allowDecimals: false
                        //labelStyle: 'width: 30%',
                        //width: '32%',
                        //maskRe: /[0-9]/,
                        //regexText: '只能輸入數字',
                        //regex: /^0|[1-9]\d*(\.\d*)?$/,

                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '計量單位',
                        name: 'BASE_UNIT',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '目前庫存量',
                        name: 'INV_QTY',
                        readOnly: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '批號效期註記',
                        name: 'WEXP_ID',
                        readOnly: true
                    },{
                        xtype: 'textfield',
                        fieldLabel: '批號',
                        labelSeparator: '',
                        name: 'LOT_NO',
                        readOnly: false,
                        selectOnFocus: true,
                        labelWidth: false,

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '批號',
                        labelSeparator: '',
                        name: 'LOT_NO_UDI',
                        hidden:true
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '效期',
                        labelSeparator: '',
                        name: 'EXP_DATE',
                        readOnly: false,
                        selectOnFocus: true,
                        labelWidth: false,
                        hideTrigger: true,
                        listeners: {
                            focus: function (self, event, eOpts) {
                                setTimeout(function () {
                                    self.expand();
                                }, 300);
                            },
                            select: function (self, event, eOpts) {
                                
                                self.blur();
                            }
                        }
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '效期',
                        labelSeparator: '',
                        name: 'EXP_DATE_UDI',
                        hidden: true
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '備註',
                        labelSeparator: '',
                        name: 'SUSE_NOTE',
                        readOnly: false,
                        selectOnFocus: true,
                        labelWidth: false,
                    },
                    , {
                        xtype: 'displayfield',
                        fieldLabel: '條碼',
                        name: 'BARCODE',
                        readOnly: true
                    },
                    
                    
                ]
            },
            {
                xtype: 'panel',
                border: false,
                plugins: 'responsive',
                margin: '1 0 1 0',
                responsiveConfig: {
                    'width < 600': {
                        layout: {
                            type: 'hbox',
                        }
                    },
                    'width >= 600': {
                        layout: {
                            type: 'hbox',
                            vertical: false
                        }
                    }
                },
                items: [{
                    xtype: 'button',
                    text: '<span style="font-size:16px">確定</span>',
                    height: '9vmin',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            width: '50%'
                        }
                    },
                    margin: '1',
                    handler: function () {
                        setData();
                    }
                },
                {
                    xtype: 'button',
                    text: '<span style="font-size:16px">取消</span>',
                    height: '9vmin',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            width: '50%'
                        }
                    },
                    margin: '1',
                    handler: function () {
                        isCancel = true;
                        changeCardQuery();
                        T1Query.getForm().findField('P1').focus();
                    }
                }]
            }
        ]

    });

    if (use_type == 'B') {
        Ext.getCmp('tratio_name').setFieldLabel('繳回量');
    } else {
        Ext.getCmp('tratio_name').setFieldLabel('扣庫量');
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Card',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Query, itemDetail]
        }]
    });
    enableField('P1');
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
});