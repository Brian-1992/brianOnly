Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var reportUrl = '/Report/A/AB0071.aspx';

    var Wh_noComboGet = '../../../api/AB0071/GetWh_noCombo';

    // 庫別清單
    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });


    function setComboData() {
        Ext.Ajax.request({
            url: Wh_noComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        for (var i = 0; i < wh_no.length; i++) {
                            Wh_noQueryStore.add({ VALUE: wh_no[i].VALUE, TEXT: wh_no[i].TEXT });
                        }
                    }
                    if (wh_no.length == 1)
                    {
                        T1Query.getForm().findField('WH_NO').setValue(wh_no[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '藥品院內碼',
        labelWidth:85,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0071/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

            }
        }
    });

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
                    xtype: 'datefield',
                    fieldLabel: '查詢日期',
                    name: 'P1',
                    labelWidth: 70,
                    width: 150,
                    padding: '0 4 4 4',
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 8,
                    width: 88,
                    padding: '0 4  4',
                    labelSeparator: '',
                },
                {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    name: 'WH_NO',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: Wh_noQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    editable: false,
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                                {
                    xtype: 'checkboxfield',
                    boxLabel: '<span style="color:blue">依處方藥排序</span>',
                    padding: '0 4 0 4',
                    name: 'OrderByRXNO',
                    inputValue: '1'
                },
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '管制用藥',
                    name: 'P4',
                    enforceMaxLength: true,
                    editable: false,
                    labelWidth: 70,
                    width: 180,
                    padding: '0 4 0 4',
                    multiSelect: true,
                    store: [
                        { abbr: 'N', name: '非管制用藥' },
                        { abbr: '0', name: '其它列管藥品' },
                        { abbr: '1', name: '第一級管制用藥' },
                        { abbr: '2', name: '第二級管制用藥' },
                        { abbr: '3', name: '第三級管制用藥' },
                        { abbr: '4', name: '第四級管制用藥' },
                        { abbr: 'Y', name: '高價藥品' }
                    ],
                    displayField: 'name',
                    valueField: 'abbr',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{name}&nbsp;</div></tpl>'
                }
                , mmCodeCombo,
                {
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
                        f.findField('P1').focus(); // 進入畫面時輸入游標預設在P1
                        msglabel('訊息區:');
                        if (Wh_noQueryStore.getCount() == 1)
                        {
                            T1Query.getForm().findField('WH_NO').setValue(Wh_noQueryStore.data.items[0].data.VALUE);
                        }
                    }
                }]
        }
        ]
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

   function T1Load(clearMsg) {
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }



    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var STOCKCODE = T1Query.getForm().findField('WH_NO').getValue();   //庫房別
            var YYYYMMDD_B = T1Query.getForm().findField('P1').rawValue; //申購日期(起)
            var YYYYMMDD_E = T1Query.getForm().findField('P2').rawValue; //申購日期(迄)
            var P4 = T1Query.getForm().findField('P4').getValue(); //管制用藥
            var MMCODE = T1Query.getForm().findField('MMCODE').getValue(); //管制用藥
            var E_RestrictCode = T1Query.getForm().findField('P4').rawValue;
            var WH_NO = T1Query.getForm().findField('WH_NO').rawValue;   //庫房別
            var OrderByRXNO = T1Query.getForm().findField('OrderByRXNO').checked
            //

            var qstring = 'STOCKCODE=' + STOCKCODE + '&YYYYMMDD_B=' + YYYYMMDD_B + '&YYYYMMDD_E=' + YYYYMMDD_E + '&P4=' + P4 + '&MMCODE=' + MMCODE + '&E_RestrictCode=' + E_RestrictCode + '&WH_NO=' + WH_NO + '&OrderByRXNO=' + OrderByRXNO;
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

});
