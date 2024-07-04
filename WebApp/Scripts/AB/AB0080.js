Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var reportUrl = '/Report/A/AB0080.aspx';
    var WhGComboGet = '../../../api/AB0080/GetWhGCombo';

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        fieldLabel: '庫別',
        fieldCls: 'required',
        padding: '0 4 0 4',
        labelWidth: 40,
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0080/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                p1: T1Query.getForm().findField('WHG').getValue()
            };

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });


    // 庫房清單
    var whGQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: WhGComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whGQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('WHG').setValue('2');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();


    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '年月份',
                    name: 'YYYYMM',
                    labelWidth: 60,
                    width: 150,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            newValue.setMonth(newValue.getMonth() - 1);
                            T1Query.getForm().findField('YYYYMM_D').setValue(newValue);
                        }
                    }
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '年月份',
                    name: 'YYYYMM_D',
                    hidden: true
                }, {
                    xtype: 'combo',
                    store: whGQueryStore,
                    fieldLabel: '庫別等級',
                    name: 'WHG',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    fieldCls: 'required',
                    multiSelect: false,
                    blankText: "請選擇庫別等級",
                    labelWidth: 70,
                    width: 220,
                    //value: '2',
                    padding: '0 10 0 4',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'fieldcontainer',
                    defaultType: 'checkboxfield',
                    labelSeparator: '',
                    padding: '0 4 0 4',
                    width: 80,
                    items: [
                        {
                            boxLabel: '等級全部',
                            name: 'WHGAll',
                            inputValue: '1',
                            listeners: {
                                change: function (ckb) {
                                    T1Query.getForm().findField('WH_NO').setDisabled(ckb.checked);
                                    T1Query.getForm().findField('WH_NO').allowBlank = (ckb.checked == true);
                                }
                            }
                        }
                    ]

                },
                wh_NoCombo
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '藥品院內碼',
                    name: 'MMCODE_B',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4',
                    labelWidth: 80,
                    width: 215
                }, {
                    xtype: 'textfield',
                    labelSeparator: '',
                    fieldLabel: '~',
                    name: 'MMCODE_E',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 20 0 4',
                    labelWidth: 15,
                    width: 150
                }, {
                    xtype: 'button',
                    text: '查詢', handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                            return;
                        }
                        reportAutoGenerate(true);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('YYYYMM').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
            ]
        }]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        //resizeTabs: true,
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }]
    });





    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var YYYYMM = T1Query.getForm().findField('YYYYMM').rawValue; //年月
            var YYYYMM_D = T1Query.getForm().findField('YYYYMM_D').rawValue; //年月
            var WHG = T1Query.getForm().findField('WHG').getValue(); //庫別等級
            var WH_NO = T1Query.getForm().findField('WH_NO').getValue(); //庫別代碼
            var WHGAll = T1Query.getForm().findField('WHGAll').checked; //等級全部
            var MMCODE_B = T1Query.getForm().findField('MMCODE_B').getValue(); //藥品月內碼B
            var MMCODE_E = T1Query.getForm().findField('MMCODE_E').getValue(); //藥品月內碼E

            var qstring = 'YYYYMM=' + YYYYMM + '&WHG=' + WHG + '&WH_NO=' + WH_NO + '&WHGAll=' + WHGAll + '&MMCODE_B=' + MMCODE_B + '&MMCODE_E=' + MMCODE_E + '&YYYYMM_D=' + YYYYMM_D;
            Ext.getDom('mainContent').src = reportUrl + '?' + qstring;
        }
    };




    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            },
            {
                region: 'center',
                xtype: 'form',
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
            }
        ]
    });

    reportAutoGenerate(false);

    T1Query.getForm().findField('YYYYMM').focus();
});
