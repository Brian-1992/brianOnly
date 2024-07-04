Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Name = "衛保室每季採購資料轉檔";
    var MatClassGet = '../../../api/FA0034/GetMatclassCombo';
    var T1GetExcel = '../../../api/FA0034/Excel';

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
                    xtype: 'combo',
                    store: matClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    name: 'P0',
                    id: 'P0',
                    allowBlank:false,
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    fieldCls:'required',
                    width: 200,
                    labelWidth: 62,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '查詢年月',
                    name: 'P1',
                    id: 'P1',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    maxValue: new Date(),
                    allowBlank: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            
                            if (newValue == null || newValue == '') {
                                return;
                            }
                            if (newValue.length < 5) {
                                return;
                            }

                            var result = dateCompare(T1Query.getForm().findField('P1').rawValue, T1Query.getForm().findField('P2').rawValue)

                            if (isInit == false && result == '>') {
                                Ext.Msg.alert('提醒', '所選月份大於後方月份');
                                if (oldValue == null || oldValue.length < 5) {
                                    self.setValue(new Date());
                                } else {
                                    self.setValue(oldValue);
                                }
                            }
                        }
                    }
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    width: 90,
                    labelWidth: 10,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    minValue: new Date(new Date().setMonth(new Date().getMonth() - 3)),
                    allowBlank: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            
                            if (newValue == null || newValue == '') {
                                return;
                            }
                            if (newValue.length < 5) {
                                return;
                            }

                            var result = dateCompare(T1Query.getForm().findField('P2').rawValue, T1Query.getForm().findField('P1').rawValue)

                            if (isInit == false && result == '<') {
                                Ext.Msg.alert('提醒', '所選月份小於前方月份');
                                if (oldValue == null || oldValue.length < 5) {
                                    self.setValue(new Date());
                                } else {
                                    self.setValue(oldValue);
                                }
                            }
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load(true);

                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>與<span style=\'color:red\'>查詢年月</span>為必填');
                        }
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                        setDefaultDate();
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
                        for (var i = 0; i < matclasses.length; i++) {
                            matClassStore.add({ VALUE: matclasses[i].VALUE, TEXT: matclasses[i].TEXT });
                        }
                        T1Query.getForm().findField("P0").setValue(matclasses[0]);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setDefaultDate() {
        var today = new Date();
        var pre = new Date(new Date().setMonth(new Date().getMonth() - 3));

        T1Query.getForm().findField('P1').setValue(pre);
        T1Query.getForm().findField('P2').setValue(today);
    }

    function dateCompare(ym1, ym2) {
        
        var date1 = getDateFromTM(ym1);
        var date2 = getDateFromTM(ym2);

        if (date1 > date2) {
            return '>';
        } else if (date1 < date2) {
            return '<';
        } else if (date1 = date2) {
            return '=';
        } else {
            return '';
        }
    }
    function getDateFromTM(ym) {
        var yyyy = Number(ym.substring(0, 3)) + 1911;
        var mm = Number(ym.substring(3, 5)) - 1;
        return new Date(yyyy, mm);
    }

    var T1Store = Ext.create('WEBAPP.store.FA0034', { // 定義於/Scripts/app/store/F/FA0034.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
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
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    if (T1Query.getForm().isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>與<span style=\'color:red\'>查詢年月</span>為必填');
                        return;
                    }
                    //var length = T1Grid.getStore().data.items.length;
                    //if (length < 1) {
                    //    Ext.Msg.alert('提示', '無資料可供匯出');
                    //    return;
                    //}

                    var filename = getFileNameDate();
                    
                    var p = new Array();
                    p.push({ name: 'FN', value: filename + '_衛保室採購資料轉檔.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: getDateString(T1Query.getForm().findField('P1').getValue()) });
                    p.push({ name: 'p2', value: getDateString(T1Query.getForm().findField('P2').getValue()) });

                    PostForm(T1GetExcel, p);
                }
            }
        ]
    });
    var getFileNameDate = function () {
        var d1 = T1Query.getForm().findField('P1').getValue();
        var d2 = T1Query.getForm().findField('P2').getValue();
        return getYYYMMDateString(d1) + "-" + getYYYMMDateString(d2);
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
        }, {
            text: "軍聯項次號",
            //text: "<span class='custom-header'>Price</span>", 
            dataIndex: 'E_ITEMARMYNO',
            width: 100,
            
        }, {
                text: "軍聯項次組別",
                dataIndex: 'E_CLFARMYNO',
            width: 100
        }, {
            text: "次數",
            dataIndex: 'CNT',
            width: 70,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "申購量",
            dataIndex: 'QTY',
            width: 70,
            align: 'right',
            style: 'text-align:left'
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
            },
            //{
            //    itemId: 'form',
            //    region: 'east',
            //    collapsible: true,
            //    floatable: true,
            //    width: 400,
            //    title: '查詢',
            //    border: false,
            //    layout: {
            //        type: 'fit',
            //        padding: 5,
            //        align: 'stretch'
            //    },
            //    items: [T1QueryForm]
            //    }
        ]
    });

    getMatclassCombo();

    setDefaultDate();
    isInit = false;

});
