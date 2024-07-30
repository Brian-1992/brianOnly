Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Name = "中央庫房差異分析報表";
    var MatClassGet = '../../../api/FA0005/GetMatclassCombo';
    var T1GetExcel = '../../../api/FA0005/Excel';
    var reportUrl = '/Report/F/FA0005.aspx';

    var T1Rec = 0;
    var T1LastRec = null;
    var isInit = true;

    // 物料分類清單
    var matClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 查詢欄位
    var mLabelWidth = 70;
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
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [

                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    value: new Date(),
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    store: matClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P1',
                    allowBlank: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 200,
                    labelWidth: 62,
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'radiogroup',
                    name: 'P2',
                    fieldLabel: '',
                    padding: '0 4 0 4',
                    items: [
                        { boxLabel: '非庫備', name: 'P2', inputValue: '0', width: 70, checked: true },
                        { boxLabel: '庫備品', name: 'P2', inputValue: '1' }
                    ]
                    //labelWidth: 100,
                    //width: 250
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load(true);

                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                        }
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                    }
                }
            ]
        }]
    });

    function getMatclassCombo() {
        Ext.Ajax.request({
            url: MatClassGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasses = data.etts;
                    if (matclasses.length > 0) {
                        matClassStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < matclasses.length; i++) {
                            matClassStore.add({ VALUE: matclasses[i].VALUE, TEXT: matclasses[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Store = Ext.create('WEBAPP.store.FA0005', { // 定義於/Scripts/app/store/F/FA0005.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

    });
    function T1Load(clearMsg) {
        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print',
                text: '列印',
                handler: function () {
                    if (T1Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                        return;
                    }
                    showReport();
                }
            },
            {
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    
                    if (T1Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                        return;
                    }
                    //var length = T1Grid.getStore().data.items.length;
                    //if (length < 1) {
                    //    Ext.Msg.alert('提示', '無資料可供匯出');
                    //    return;
                    //}

                    //var filename = getFileNameDate();
                    
                    var p = new Array();
                    p.push({ name: 'FN', value: T1Query.getForm().findField('P0').rawValue + '_中央庫房差異分析報表.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue()['P2'] });

                    PostForm(T1GetExcel, p);
                }
            }
        ]
    });
    function showReport(record) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?CHK_YM=' + T1Query.getForm().findField('P0').rawValue
                + '&CHK_YYYYMM=' + dateTransform(T1Query.getForm().findField('P0').getValue())
                + '&MAT_CLASS=' + T1Query.getForm().findField('P1').getValue()
                + '&M_STOREID=' + T1Query.getForm().findField('P2').getValue()['P2']
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }
    function dateTransform(date) {
        var yyyy = date.getFullYear().toString();
        var m = (date.getMonth() + 1).toString();
        var d = date.getDate().toString();
        var mm = m.length < 2 ? "0" + m : m;
        var dd = d.length < 2 ? "0" + d : d;
        return yyyy + "-" + mm + "-" + dd;
    }

    var getFileNameDate = function () {
        var d1 = T1Query.getForm().findField('P0').getValue();
        return getYYYMMDateString(d1);
    }
    function getYYYMMDateString(date) {
        var yyy = date.getFullYear() - 1911;
        var m = (date.getMonth() + 1).toString();
        var mm = m.length < 2 ? "0" + m : m;
        return yyy.toString() + mm;
    }
    var getDateString = function (date) {
        var y = (date.getFullYear()).toString();
        var m = (date.getMonth() + 1).toString();
        var d = (date.getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + "-" + m + "-" + d;
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'panel',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, 
        {
            text: "庫存數量",
            dataIndex: 'STORE_QTY',
            width: 100,
            align: 'right',
            style: 'text-align:left'
        }, 
        {
            text: "庫存成本",
            dataIndex: 'STORE_COST',
            width: 100,
            align: 'right',
            style: 'text-align:left'
        }, 
        {
            text: "實盤數量",
            dataIndex: 'CHK_QTY',
            width: 100,
            align: 'right',
            style: 'text-align:left'
        }, 
        {
            text: "實盤成本",
            dataIndex: 'CHK_COST',
            width: 100,
            align: 'right',
            style: 'text-align:left'
        }, 
        {
            text: "差異成本",
            dataIndex: 'DIFF_COST',
            width: 100,
            align: 'right',
            style: 'text-align:left'
        },
        {
            text: "差異分析說明",
            //text: "<span class='custom-header'>Price</span>", 
            dataIndex: 'COMMENT',
            width: 150,

        }, {
            header: "",
            flex: 1
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
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }
        ]
    });

    getMatclassCombo();

});
