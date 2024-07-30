Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAB = Ext.getUrlParam('isAB');

    var St_WhGet = '../../../api/AA0152/GetWhCombo';
    var St_MatclassGet = '../../../api/AA0152/GetMatclassCombo';
    var T1GetExcel = '../../../api/AA0152/Excel';

    // 庫房
    var st_wh = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 物料分類
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        allowBlank: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0152/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            if (!f.findField("P1").readOnly) {
                var mat_class = f.findField("P1").getValue();
                return {
                    MAT_CLASS: mat_class
                };
            }
        },
        listeners: {
        }
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
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
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房',
                        id: 'P0',
                        name: 'P0',
                        store: st_wh,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        multiSelect: false,
                        width: 250,
                        listeners: {
                            select: function (ele, newValue, oldValue) {
                                setComboMatClass();
                            }
                        }
                    },
                    {
                        xtype: 'tbspacer',
                        width: 20
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        id: 'P1',
                        name: 'P1',
                        store: st_matclass,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        multiSelect: false,
                        width: 200,
                        listeners: {
                        },
                    },
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        items: [
                            mmCodeCombo,
                        ]
                    },
                    {
                        xtype: 'tbspacer',
                        width: 30
                    },
                    {
                        xtype: 'monthfield',
                        fieldLabel: '效期區間',
                        id: 'D0',
                        name: 'D0',
                        width: 160
                    },
                    {
                        xtype: 'tbspacer',
                        width: 10
                    },
                    {
                        xtype: 'monthfield',
                        fieldLabel: '至',
                        id: 'D1',
                        name: 'D1',
                        labelWidth: 20,
                        width: 120
                    },
                    {
                        xtype: 'checkbox',
                        boxLabel: '效期9991231',
                        id: 'P3',
                        name: 'P3',
                        labelWidth: 100,
                        width: 140,
                        margin: '0 0 0 10',
                        checked: true,
                        hidden: isAB? false : true 
                    },

                    {
                        xtype: 'tbspacer',
                        width: 10
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load,
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            T1Query.getForm().findField('P0').reset();
                            T1Query.getForm().findField('P1').reset();
                            T1Query.getForm().findField('P2').reset();
                            T1Query.getForm().findField('D0').reset();
                            T1Query.getForm().findField('D1').reset();

                            var f = this.up('form').getForm();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }]
        }]
    });

    function setComboWhNo() {
        st_wh.removeAll();
        //庫房
        Ext.Ajax.request({
            url: St_WhGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_wh = data.etts;
                    if (tb_wh.length > 0) {
                        for (var i = 0; i < tb_wh.length; i++) {
                            st_wh.add({ VALUE: tb_wh[i].VALUE, COMBITEM: tb_wh[i].COMBITEM });
                        }
                    }
                    else {
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setComboMatClass() {
        st_matclass.removeAll();
        T1Query.getForm().findField('P1').setValue('');
        var wh_no = T1Query.getForm().findField('P0').getValue();
        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            params: {
                p0: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }
                        if (tb_matclass.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('P1').setValue(tb_matclass[0].VALUE);
                        }
                        else {
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setDefaultExpDate() {
        //效期區間
        Ext.Ajax.request({
            url: '/api/AA0152/GetExpDate',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_expDate = data.etts;
                    if (tb_expDate.length > 0) {
                        T1Query.getForm().findField('D0').setValue(tb_expDate[0].VALUE);
                        T1Query.getForm().findField('D1').setValue(tb_expDate[0].TEXT);
                    }
                    else {
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboWhNo();
    setComboMatClass();
    setDefaultExpDate();

    //T1Model        //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' }, // 庫房代碼
            { name: 'WH_NAME', type: 'string' }, // 庫房名稱
            { name: 'MMCODE', type: 'string' },        // 院內碼
            { name: 'MMCODE_C', type: 'string' },        // 中文品名
            { name: 'MMCODE_E', type: 'string' },        // 英文品名
            { name: 'LOT_NO', type: 'string' },        // 批號
            { name: 'EXP_DATE', type: 'string' },        // 效期
            { name: 'INV_QTY', type: 'string' },        // 存量
            { name: 'EXP_CHK', type: 'string' },        // 是否近效期
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0152/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                } else {
                    msglabel('查無符合條件的資料!');
                    Ext.getCmp('export').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').rawValue);
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').rawValue);
        T1Store.getProxy().setExtraParam("isab", isAB ? "Y" : "N");
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        displayMsg: "顯示{0} - {1}筆,共{2}筆       <span style=\'color:red\'>*效期在180天內以紅字顯示</span>",
        buttons: [
            {
                id: 'export',
                text: '匯出',
                disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '效期資料查詢' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'd0', value: T1Query.getForm().findField('D0').rawValue });
                    p.push({ name: 'd1', value: T1Query.getForm().findField('D1').rawValue });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });

                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
        ]
    });

    // 查詢結果資料列表
    //1130116 804要求若紅字則整行紅字
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 110,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['WH_NO'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['WH_NO'];
                }
            },
            {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['WH_NAME'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['WH_NAME'];
                }
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['MMCODE'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['MMCODE'];
                }
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 350,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['MMNAME_C'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['MMNAME_C'];
                }
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 350,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['MMNAME_E'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['MMNAME_E'];
                }
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['LOT_NO'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['LOT_NO'];
                }
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                style: 'text-align:left',
                align: 'left',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['EXP_DATE'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['EXP_DATE'];
                }
            },
            {
                text: "存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['EXP_CHK'] == 'Y')
                        return '<font color=red>' + record.data['INV_QTY'] + '</font>'; // 近效期以紅字顯示
                    else
                        return record.data['INV_QTY'];
                }
            },
        ],
        listeners: {
            renderer: function (val, meta, record) {
                if (record.data['EXP_CHK'] == 'Y')
                    return '<font color=red>' + record.data['EXP_DATE'] + '</font>'; // 近效期以紅字顯示
                else
                    return record.data['EXP_DATE'];
            }
        }
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

    T1Query.getForm().findField('P0').focus();
    Ext.getCmp('export').setDisabled(true);
});