Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var DeptQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            url: '/api/AA0169/GetDeptCombo',
            reader: {
                type: 'json',
                root: 'etts'
            }
        },
        autoLoad: true
    });


    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        layout: {
            type: 'vbox',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P0',
                    id: 'P0',
                    allowBlank: false,
                    fieldCls: 'required',
                    width: 160
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫別',
                    store: DeptQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 60,
                    width: 200,
                    allowBlank: false,
                    fieldCls: 'required',
                    listConfig:
                    {
                        width: 200
                    },
                    matchFieldWidth: false
                }, {
                    xtype: 'textfield',
                    fieldLabel: '幾個月前未異動',
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 110,
                    width: 200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '消耗量',
                    name: 'P2',
                    id: 'P2',
                    labelWidth: 60,
                    width: 160

                }, {
                    xtype: 'button',
                    text: '列印',
                    handler: function () {
                        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null)) {

                            Ext.Msg.alert('訊息', '需填查詢月份才能匯出');
                            return;
                        }
                        if ((T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)) {

                            Ext.Msg.alert('訊息', '需填庫別才能匯出');
                            return;
                        }
                        reportUrl = '/Report/A/AA0169.aspx';
                        showReport();

                    }

                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P1=' + T1Query.getForm().findField('P1').getValue()
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P3=' + T1Query.getForm().findField('P3').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    var T1Store = Ext.create('WEBAPP.store.MiUnitcodeAA0119', { // 定義於/Scripts/app/store/MiUnitcodeAA0119.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }],
    });

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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });
});